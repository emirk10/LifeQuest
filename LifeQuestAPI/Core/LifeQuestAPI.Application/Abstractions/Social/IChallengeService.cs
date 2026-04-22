using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeQuestAPI.Application.Abstractions.Social;

public interface IChallengeService
{
    Task UpdateChallengeProgressAsync(Guid userId, Guid? categoryId);
}
