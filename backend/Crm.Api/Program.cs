using System;
using Microsoft.AspNetCore.Http;
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
using Crm.Infrastructure.Services;
using Serilog;
using Hangfire.MemoryStorage;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.HttpOverrides;
using Crm.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Resolve Connection String & Provider
var rawDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var isUsingRailway = !string.IsNullOrEmpty(rawDatabaseUrl);

// If no production URL is found, we assume we're in 'Local First' mode unless explicitly told otherwise.
bool useSqlite = !isUsingRailway || builder.Configuration["Database:Provider"] == "Sqlite";

var connectionString = "";

if (useSqlite)
{
    connectionString = "Data Source=crm_local.db";
    Log.Information("🚀 Using Local SQLite (zero-config). Database: {ConnectionString}", connectionString);
}
else
{
    connectionString = ParseDatabaseUrl(rawDatabaseUrl!); 
    Log.Information("🌍 Using PostgreSQL provider (Railway).");
}

var rawAllowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") 
    ?? builder.Configuration["CORS:AllowedOrigins"];

var allowedOrigins = rawAllowedOrigins?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) 
    ?? new[] { "http://localhost:3000", "https://agency-ccrm.netlify.app", "https://crm.tradocademy.ng" };

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

Log.Information("🚀 Starting Agency CRM API in {Environment} mode", builder.Environment.EnvironmentName);
Log.Information("🌐 Allowed CORS Origins: {Origins}", string.Join(", ", allowedOrigins));

builder.Services.AddControllers();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// EF Core Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (useSqlite)
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

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
builder.Services.AddScoped<IContractPortalService, ContractPortalService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<TimeTrackingService>();
builder.Services.AddScoped<IAutomationService, AutomationService>();
builder.Services.AddScoped<IAdMetricService, AdMetricService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// Ad Platforms
builder.Services.AddHttpClient<GoogleAdsClient>();
builder.Services.AddHttpClient<MetaAdsClient>();
builder.Services.AddScoped<IAdPlatformClient>(sp => sp.GetRequiredService<GoogleAdsClient>());
builder.Services.AddScoped<IAdPlatformClient>(sp => sp.GetRequiredService<MetaAdsClient>());

// Notifications
builder.Services.AddHttpClient<ISlackService, SlackService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISlackService, SlackService>();

// Background Jobs
builder.Services.AddScoped<AdMetricsSyncJob>();
builder.Services.AddScoped<RemindersJob>();
builder.Services.AddScoped<AutomationJobs>();

// Hangfire Configuration
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings();

    if (useSqlite)
    {
        config.UseMemoryStorage();
    }
    else
    {
        config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
    }
});

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing."))),
            NameClaimType = "sub",
            RoleClaimType = "role"
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

// Configure Forwarded Headers for Railway Proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Seed Database (Always runs if database is empty, even in Production)
try 
{
    using (var scope = app.Services.CreateScope())
    {
        await DbInitializer.SeedAsync(scope.ServiceProvider);
    }
}
catch (Exception ex)
{
    Log.Error(ex, "[CRITICAL STARTUP ERROR] DbInitializer.SeedAsync failed: {Message}", ex.Message);
}

// Migrate Database on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        if (useSqlite)
        {
            db.Database.EnsureCreated();
            Log.Information("SQLite Database migrated successfully.");
        }
        else if (db.Database.GetPendingMigrations().Any())
        {
            Log.Information("Applying pending PostgreSQL migrations...");
            db.Database.Migrate();
            Log.Information("PostgreSQL Migrations applied successfully.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
    
    // Allow localhost, production frontend, and tracking academy domain
    var csp = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; " +
              "connect-src 'self' http://localhost:* ws://localhost:* https://agency-ccrm.netlify.app https://crm.tradocademy.ng; " +
              "frame-ancestors 'none';";
    context.Response.Headers.Append("Content-Security-Policy", csp);
    await next();
});

// Use the "DefaultPolicy" configured above
app.UseCors("DefaultPolicy");

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

        recurringJobManager.AddOrUpdate<AutomationJobs>(
            "overdue-invoice-check",
            job => job.CheckOverdueInvoicesJob(),
            config["OverdueInvoiceCheckInterval"] ?? Cron.Daily());

    recurringJobManager.AddOrUpdate<AutomationJobs>(
        "monthly-billing",
        job => job.MonthlyBillingJob(),
        config["MonthlyBillingInterval"] ?? Cron.Monthly(1));
}

app.MapGet("/", () => Results.Ok(new 
{ 
    message = "Agency CRM API is running", 
    status = "healthy",
    documentation = "/swagger",
    timestamp = DateTime.UtcNow 
}));

app.MapControllers();

// Diagnostics Endpoints
app.MapGet("/ping", () => Results.Ok(new { status = "healthy", message = "pong", timestamp = DateTime.UtcNow }));

app.MapGet("/api/health/db", async (AppDbContext db) => {
    try {
        var canConnect = await db.Database.CanConnectAsync();
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
        
        // Also check if critical tables exist
        var hasTenants = false;
        try { hasTenants = await db.Tenants.AnyAsync(); } catch { }

        return Results.Ok(new { 
            status = "healthy", 
            database = canConnect ? "connected" : "disconnected",
            provider = useSqlite ? "SQLite" : "PostgreSQL",
            environment = isUsingRailway ? "Railway" : "Local",
            appliedMigrations = appliedMigrations,
            pendingMigrations = pendingMigrations,
            hasTenantsTable = hasTenants,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex) {
        return Results.Problem(
            title: "Database Health Check Failed",
            detail: ex.Message,
            statusCode: 500
        );
    }
}).RequireAuthorization(p => p.RequireRole("Admin"));

app.Run();

public partial class Program 
{ 
    private static string? ParseDatabaseUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (!url.StartsWith("postgres://") && !url.StartsWith("postgresql://")) return url;

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        // Smart SSL: Internal Railway URLs usually don't support/require SSL.
        // External URLs (connecting to Railway from outside) usually DO require it.
        var isInternal = host.EndsWith(".railway.internal");
        var sslMode = isInternal ? "Disable" : "Require";
        var trustCertificate = !isInternal; // Trust server cert for external cloud connections

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode={sslMode};Trust Server Certificate={trustCertificate}";
    }
}
