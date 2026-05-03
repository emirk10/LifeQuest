using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommandRequest, VerifyEmailCommandResponse>
{
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;

    public VerifyEmailCommandHandler(IAppUserWriteRepository userWriteRepository, IAppUserReadRepository userReadRepository)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
    }

    public async Task<VerifyEmailCommandResponse> Handle(VerifyEmailCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetSingleAsync(u => u.EmailVerificationToken == request.Token, tracking: true);

        if (user == null)
        {
            return new VerifyEmailCommandResponse
            {
                IsSuccess = false,
                Message = "Geçersiz doğrulama tokeni."
            };
        }

        if (user.IsEmailVerified)
        {
            return new VerifyEmailCommandResponse
            {
                IsSuccess = false,
                Message = "Email adresi zaten doğrulanmış."
            };
        }

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return new VerifyEmailCommandResponse
            {
                IsSuccess = false,
                Message = "Doğrulama bağlantısının süresi dolmuş."
            };
        }

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        return new VerifyEmailCommandResponse
        {
            IsSuccess = true,
            Message = "Email adresi başarıyla doğrulanmıştır."
        };
    }
}
