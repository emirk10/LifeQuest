using LifeQuestAPI.Application.Abstractions.Token;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommandRequest, RefreshTokenCommandResponse>
{
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IAppUserWriteRepository userWriteRepository, 
        IAppUserReadRepository userReadRepository,
        ITokenService tokenService)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _tokenService = tokenService;
    }

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetAll(tracking: true)
                                     .Include(u => u.AppRole)
                                     .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null)
        {
            return new RefreshTokenCommandResponse
            {
                IsSuccess = false,
                Message = "Geçersiz refresh token."
            };
        }

        if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            return new RefreshTokenCommandResponse
            {
                IsSuccess = false,
                Message = "Refresh token'ın süresi dolmuş. Lütfen tekrar giriş yapın."
            };
        }

        
        string newAccessToken = _tokenService.CreateToken(user);
        string newRefreshToken = _tokenService.CreateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = expiry;

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        return new RefreshTokenCommandResponse
        {
            IsSuccess = true,
            Message = "Token başarıyla yenilendi.",
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiration = expiry
        };
    }
}
