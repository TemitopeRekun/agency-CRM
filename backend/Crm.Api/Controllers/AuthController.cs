using Crm.Application.DTOs.Auth;
using Crm.Application.Services;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Crm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUserContext _userContext;

    public AuthController(AuthService authService, ILogger<AuthController> logger, IWebHostEnvironment env, ICurrentUserContext userContext)
    {
        _authService = authService;
        _logger = logger;
        _env = env;
        _userContext = userContext;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Email}", request.Email);
        var (response, accessToken, refreshToken) = await _authService.LoginAsync(request, GetIpAddress());

        if (response == null)
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        _logger.LogInformation("User {Email} logged in successfully", request.Email);
        SetTokenCookie("refresh_token", refreshToken!);
        SetTokenCookie("access_token", accessToken!);

        response.AccessToken = accessToken!;
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken)) return BadRequest("Token is required.");

        var (response, accessToken, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken, GetIpAddress());

        if (response == null) return Unauthorized("Invalid token.");

        SetTokenCookie("refresh_token", newRefreshToken!);
        SetTokenCookie("access_token", accessToken!);

        response.AccessToken = accessToken!;
        return Ok(response);
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeTokenAsync(refreshToken, GetIpAddress());
        }

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");

        return Ok(new { Message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = _userContext.UserId;
        if (!userId.HasValue) return Unauthorized();

        var response = await _authService.GetMeAsync(userId.Value);
        if (response == null) return Unauthorized();

        return Ok(response);
    }

    private void SetTokenCookie(string name, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = name == "refresh_token" ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(15),
            Secure = !_env.IsDevelopment(), // Secure cookies in production/staging (requires HTTPS)
            SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None, // 'None' is required for cross-domain cookies in production
            Path = "/" // Explicitly set path to root so all API endpoints can receive the cookie
        };
        Response.Cookies.Append(name, token, cookieOptions);
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"]!;
        else
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "N/A";
    }
}
