using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Common.Security;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities.Identity;
using MediatR;

using LifeQuestAPI.Application.Abstractions.Email;

namespace LifeQuestAPI.Application.Features.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommandRequest, RegisterUserCommandResponse>
{
    private readonly IAppUserWriteRepository _userWriteRepository;
    private readonly IAppUserReadRepository _userReadRepository;
    private readonly IAppRoleReadRepository _roleReadRepository;
    private readonly IEmailService _emailService;

    public RegisterUserCommandHandler(
        IAppUserWriteRepository userWriteRepository, 
        IAppUserReadRepository userReadRepository, 
        IAppRoleReadRepository roleReadRepository,
        IEmailService emailService)
    {
        _userWriteRepository = userWriteRepository;
        _userReadRepository = userReadRepository;
        _roleReadRepository = roleReadRepository;
        _emailService = emailService;
    }

    public async Task<RegisterUserCommandResponse> Handle(RegisterUserCommandRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userReadRepository.GetSingleAsync(u => u.Email == request.Email, tracking: false);
        if (existingUser != null)
            throw new Exception("Bu email adresi zaten kullanılıyor.");

        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        var verificationToken = Guid.NewGuid().ToString();

        var newUser = new AppUser
        {
            Name = request.Name,
            Surname = request.Surname,
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            TotalXP = 0,
            GeneralLevel = 1,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        var defaultRole = await _roleReadRepository.GetSingleAsync(r => r.Name == "User", tracking: false);

        if (defaultRole != null)
        {
            newUser.AppRoleId = defaultRole.Id;
        }
        else
        {
            newUser.AppRole = new AppRole { Name = "User" };
        }

        await _userWriteRepository.AddAsync(newUser);
        await _userWriteRepository.SaveAsync();

        var verificationLink = $"http://localhost:5173/verify-email?token={verificationToken}";
        var emailBody = $@"
            <h2>LifeQuest'e Hoşgeldiniz!</h2>
            <p>Merhaba {newUser.Name},</p>
            <p>Hesabınızı doğrulamak için lütfen aşağıdaki bağlantıya tıklayın:</p>
            <p><a href='{verificationLink}'>{verificationLink}</a></p>
            <p>Bu bağlantı 24 saat geçerlidir.</p>
        ";

        await _emailService.SendEmailAsync(newUser.Email, "LifeQuest Email Doğrulama", emailBody);

        return new RegisterUserCommandResponse
        {
            IsSuccess = true,
            Message = "Kullanıcı başarıyla oluşturuldu. Giriş yapabilirsiniz."
        };
    }
}
