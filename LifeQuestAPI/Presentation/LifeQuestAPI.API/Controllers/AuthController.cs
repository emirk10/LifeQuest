using LifeQuestAPI.Application.Features.Auth.Commands.Login;
using LifeQuestAPI.Application.Features.Auth.Commands.Register;
using MediatR;
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
    public async Task<IActionResult> Register(RegisterUserCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserCommandRequest request)
    {
        var response = await _mediator.Send(request);
        return Ok(response);
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
    public async Task<IActionResult> RefreshToken(LifeQuestAPI.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommandRequest request)
    {
        var response = await _mediator.Send(request);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        return Unauthorized(response);
    }
}