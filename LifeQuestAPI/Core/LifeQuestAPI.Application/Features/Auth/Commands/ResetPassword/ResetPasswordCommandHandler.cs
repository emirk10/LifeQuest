using LifeQuestAPI.Application.Common.Security;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommandRequest, ResetPasswordCommandResponse>
{
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;

    public ResetPasswordCommandHandler(
        IAppUserWriteRepository userWriteRepository, 
        IAppUserReadRepository userReadRepository)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
    }

    public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetSingleAsync(u => u.PasswordResetToken == request.Token, tracking: true);

        if (user == null)
        {
            return new ResetPasswordCommandResponse
            {
                IsSuccess = false,
                Message = "Geçersiz şifre sıfırlama tokeni."
            };
        }

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            return new ResetPasswordCommandResponse
            {
                IsSuccess = false,
                Message = "Şifre sıfırlama bağlantısının süresi dolmuş."
            };
        }

        HashingHelper.CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);

        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        return new ResetPasswordCommandResponse
        {
            IsSuccess = true,
            Message = "Şifreniz başarıyla sıfırlandı."
        };
    }
}
