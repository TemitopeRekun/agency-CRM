using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Crm.Infrastructure.Data;
using Crm.Application.Interfaces;
using Crm.Application.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Crm.Infrastructure.BackgroundJobs;
using Serilog;
using Crm.Api.Middleware;
using Crm.Infrastructure.Monitoring;

using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// EF Core + Postgres
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Security & Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, Crm.Infrastructure.Security.CurrentUserContext>();
builder.Services.AddScoped<IUserRepository, Crm.Infrastructure.Repositories.UserRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(Crm.Infrastructure.Repositories.GenericRepository<>));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<OfferService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<TimeEntryService>();
builder.Services.AddScoped<AdMetricService>();

// Background Jobs
builder.Services.AddScoped<AdMetricsSyncJob>();
builder.Services.AddScoped<RemindersJob>();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default"))));

builder.Services.AddHangfireServer();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // Ensures raw claim names like 'tenant_id' are used
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing.")))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Prioritize Authorization Header, fallback to Cookie
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                {
                    token = context.Request.Cookies["access_token"];
                }
                context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed Database
if (app.Environment.IsDevelopment())
{
    try 
    {
        // Use a background task or just run at startup
        Task.Run(async () => {
            using var scope = app.Services.CreateScope();
            await DbInitializer.SeedAsync(scope.ServiceProvider);
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CRITICAL STARTUP ERROR] DbInitializer.SeedAsync failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Allow localhost without port for easier testing
    var csp = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' http://localhost:* ws://localhost:*; frame-ancestors 'none';";
    context.Response.Headers.Append("Content-Security-Policy", csp);
    await next();
});

var allowedOrigins = builder.Configuration["CORS:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" };
app.UseCors(policy => policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(_ => true));

// Global Exception Handling (RFC 7807)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Crm.Api.Security.HangfireAuthorizationFilter() }
});

// Schedule Recurring Jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var config = builder.Configuration.GetSection("BackgroundJobs");

    recurringJobManager.AddOrUpdate<AdMetricsSyncJob>(
        "ad-metrics-sync",
        job => job.ExecuteAsync(),
        config["AdMetricsSyncInterval"] ?? Cron.Daily());

    recurringJobManager.AddOrUpdate<RemindersJob>(
        "daily-reminders",
        job => job.ExecuteAsync(),
        config["RemindersInterval"] ?? Cron.Daily());
}

app.MapControllers();

app.Run();

public partial class Program { }
