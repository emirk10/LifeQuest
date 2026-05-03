using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Application.Abstractions.Token;

public interface ITokenService
{
    string CreateToken(AppUser user);
    string CreateRefreshToken();
}