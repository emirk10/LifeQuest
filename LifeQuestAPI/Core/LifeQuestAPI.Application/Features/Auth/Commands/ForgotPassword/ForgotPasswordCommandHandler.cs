using LifeQuestAPI.Application.Abstractions.Email;
using LifeQuestAPI.Application.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommandRequest, ForgotPasswordCommandResponse>
{
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IAppUserWriteRepository userWriteRepository, 
        IAppUserReadRepository userReadRepository, 
        IEmailService emailService)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _emailService = emailService;
    }

    public async Task<ForgotPasswordCommandResponse> Handle(ForgotPasswordCommandRequest request, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetSingleAsync(u => u.Email == request.Email, tracking: true);

        if (user == null)
        {
            // Do not reveal that the user does not exist for security reasons
            return new ForgotPasswordCommandResponse
            {
                IsSuccess = true,
                Message = "Eğer bu email adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir."
            };
        }

        var resetToken = Guid.NewGuid().ToString();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        _userWriteRepository.Update(user);
        await _userWriteRepository.SaveAsync();

        var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
        var emailBody = $@"
            <h2>Şifre Sıfırlama Talebi</h2>
            <p>Merhaba {user.Name},</p>
            <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p>Bu bağlantı 1 saat geçerlidir.</p>
            <p>Eğer bu talebi siz yapmadıysanız, bu e-postayı dikkate almayın.</p>
        ";

        await _emailService.SendEmailAsync(user.Email, "LifeQuest Şifre Sıfırlama", emailBody);

        return new ForgotPasswordCommandResponse
        {
            IsSuccess = true,
            Message = "Eğer bu email adresi sistemde kayıtlıysa, şifre sıfırlama bağlantısı gönderilmiştir."
        };
    }
}
