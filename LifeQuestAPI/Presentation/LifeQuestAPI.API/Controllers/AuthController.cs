using LifeQuestAPI.Application.Features.Auth.Commands.Login;
using LifeQuestAPI.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LifeQuestAPI.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> Register(RegisterUserCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> Login(LoginUserCommandRequest request)
    {
        var response = await _mediator.Send(request);
        
        SetTokenCookies(response.Token, response.RefreshToken, response.RefreshTokenExpiration);
        
        // Return without sensitive tokens in body
        return Ok(new { 
            response.IsSuccess, 
            response.Message,
            response.UserId,
            response.UserName,
            response.Role
        });
    }

    [HttpPatch("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var request = new LifeQuestAPI.Application.Features.Auth.Commands.VerifyEmail.VerifyEmailCommandRequest { Token = token };
        var response = await _mediator.Send(request);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("forgot-password")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthRateLimit")]
    public async Task<IActionResult> ForgotPassword(LifeQuestAPI.Application.Features.Auth.Commands.ForgotPassword.ForgotPasswordCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(LifeQuestAPI.Application.Features.Auth.Commands.ResetPassword.ResetPasswordCommandRequest request)
    {
        var response = await _mediator.Send(request);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return BadRequest(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { IsSuccess = false, Message = "Refresh token bulunamadı." });
        }

        var request = new LifeQuestAPI.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommandRequest { RefreshToken = refreshToken };
        var response = await _mediator.Send(request);
        
        if (response.IsSuccess)
        {
            SetTokenCookies(response.Token, response.RefreshToken, response.RefreshTokenExpiration);
            return Ok(new { response.IsSuccess, response.Message });
        }
        
        return Unauthorized(response);
    }

    [HttpPost("revoke")]
    public IActionResult Revoke()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        // We could also call a command to clear it in DB, but just clearing cookie is fine for frontend logout.
        return Ok(new { IsSuccess = true, Message = "Çıkış yapıldı." });
    }

    private void SetTokenCookies(string accessToken, string refreshToken, DateTime? refreshTokenExpiration)
    {
        // Secure=true yalnızca HTTPS'te çalışır. Dev/Docker ortamında HTTP kullanıldığında
        // Secure=true cookie göndermez ve auth tamamen bozulur.
        var isSecure = !HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };
        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = refreshTokenExpiration ?? DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);
    }
}