using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.Token;
using LifeQuestAPI.Application.Common.Security;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.Application.Features.Auth.Commands.Login;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommandRequest, LoginUserCommandResponse>
{
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(IAppUserReadRepository userReadRepository, IAppUserWriteRepository userWriteRepository, ITokenService tokenService)
    {
        _userReadRepository = userReadRepository;
        _userWriteRepository = userWriteRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginUserCommandResponse> Handle(LoginUserCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetAll(tracking: true)
                                     .Include(u => u.AppRole)
                                     .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            throw new Exception("Kullanıcı adı veya şifre hatalı.");

        if (!HashingHelper.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new Exception("Kullanıcı adı veya şifre hatalı.");

        string token = _tokenService.CreateToken(user);
        string refreshToken = _tokenService.CreateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiry;

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        return new LoginUserCommandResponse
        {
            IsSuccess = true,
            Message = "Giriş başarılı.",
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = expiry,
            UserId = user.Id,
            UserName = user.UserName,
            Role = user.AppRole?.Name ?? "User"
        };
    }
}