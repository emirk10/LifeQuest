using LifeQuestAPI.Application.Abstractions.Email;
using LifeQuestAPI.Application.Features.Social.Commands.SendChallengeInvite;
using LifeQuestAPI.Application.Features.Social.Commands.RespondToChallenge;
using LifeQuestAPI.Application.Features.Social.Queries.GetActiveChallenges;
using LifeQuestAPI.Application.Features.Social.Commands.GiveKudos;
using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LifeQuestAPI.API.Controllers;

[Route("api/social")]
[ApiController]
[Authorize]
public class SocialController : ControllerBase
{
    private readonly IFriendshipReadRepository _friendshipRead;
    private readonly IFriendshipWriteRepository _friendshipWrite;
    private readonly IChallengeReadRepository _challengeRead;
    private readonly IChallengeWriteRepository _challengeWrite;
    private readonly IAppUserReadRepository _userRead;
    private readonly IAppUserWriteRepository _userWrite;
    private readonly IAppTaskReadRepository _appTaskRead;
    private readonly IUserTaskReadRepository _userTaskRead;
    private readonly IUserTaskWriteRepository _userTaskWrite;
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;

    public SocialController(
        IFriendshipReadRepository friendshipRead,
        IFriendshipWriteRepository friendshipWrite,
        IChallengeReadRepository challengeRead,
        IChallengeWriteRepository challengeWrite,
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite,
        IAppTaskReadRepository appTaskRead,
        IUserTaskReadRepository userTaskRead,
        IUserTaskWriteRepository userTaskWrite,
        IEmailService emailService,
        IMediator mediator)
    {
        _friendshipRead = friendshipRead;
        _friendshipWrite = friendshipWrite;
        _challengeRead = challengeRead;
        _challengeWrite = challengeWrite;
        _userRead = userRead;
        _userWrite = userWrite;
        _appTaskRead = appTaskRead;
        _userTaskRead = userTaskRead;
        _userTaskWrite = userTaskWrite;
        _emailService = emailService;
        _mediator = mediator;
    }

    
    
    

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var query = _friendshipRead.GetAll(false)
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted);

        var totalCount = await query.CountAsync();

        var friendships = await query
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var friends = friendships.Select(f =>
        {
            var friend = f.RequesterId == userId ? f.Addressee : f.Requester;
            return new
            {
                friendshipId = f.Id,
                friendId = friend.Id,
                name = friend.Name,
                surname = friend.Surname,
                fullName = friend.FullName,
                userName = friend.UserName,
                level = friend.GeneralLevel,
                totalXP = friend.TotalXP,
                avatarId = friend.AvatarId,
                acceptedAt = f.AcceptedAt
            };
        });

        return Ok(new { success = true, friends, totalCount, page, pageSize });
    }

    [HttpGet("friend-requests")]
    public async Task<IActionResult> GetFriendRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var query = _friendshipRead.GetAll(false)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending);

        var totalCount = await query.CountAsync();

        var requests = await query
            .Include(f => f.Requester)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                requestId = f.Id,
                requesterId = f.RequesterId,
                requesterName = f.Requester.Name,
                requesterSurname = f.Requester.Surname,
                requesterUserName = f.Requester.UserName,
                requesterLevel = f.Requester.GeneralLevel,
                sentAt = f.CreatedAt
            })
            .ToListAsync();

        return Ok(new { success = true, requests, totalCount, page, pageSize });
    }

    [HttpPost("friend-request/{targetUserId}")]
    public async Task<IActionResult> SendFriendRequest(Guid targetUserId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (userId == targetUserId)
            return BadRequest(new { success = false, message = "Kendinize istek gönderemezsiniz." });

        var targetUser = await _userRead.GetByIdAsync(targetUserId.ToString());
        if (targetUser == null)
            return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

        var existing = await _friendshipRead.GetAll(false)
            .FirstOrDefaultAsync(f =>
                (f.RequesterId == userId && f.AddresseeId == targetUserId) ||
                (f.RequesterId == targetUserId && f.AddresseeId == userId));

        if (existing != null)
        {
            if (existing.Status == FriendshipStatus.Rejected)
            {
                
                existing.RequesterId = userId.Value;
                existing.AddresseeId = targetUserId;
                existing.Status = FriendshipStatus.Pending;
                existing.UpdatedAt = DateTime.UtcNow;
                _friendshipWrite.Update(existing);
                await _friendshipWrite.SaveAsync();
                return Ok(new { success = true, message = "Arkadaşlık isteği gönderildi!" });
            }

            if (existing.Status == FriendshipStatus.Pending && existing.AddresseeId == userId)
            {
                
                existing.Status = FriendshipStatus.Accepted;
                existing.AcceptedAt = DateTime.UtcNow;
                _friendshipWrite.Update(existing);
                await _friendshipWrite.SaveAsync();
                return Ok(new { success = true, message = "Arkadaşlık isteği kabul edildi! 🎉" });
            }

            var msg = existing.Status == FriendshipStatus.Accepted
                ? "Zaten arkadaşsınız."
                : "Zaten bir istek gönderilmiş veya mevcut.";
            return BadRequest(new { success = false, message = msg });
        }

        
        var deletedFriendship = await _friendshipRead.GetAll(false)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f =>
                f.IsDeleted &&
                ((f.RequesterId == userId && f.AddresseeId == targetUserId) ||
                 (f.RequesterId == targetUserId && f.AddresseeId == userId)));

        if (deletedFriendship != null)
        {
            
            deletedFriendship.RequesterId = userId.Value;
            deletedFriendship.AddresseeId = targetUserId;
            deletedFriendship.Status = FriendshipStatus.Pending;
            deletedFriendship.IsDeleted = false;
            deletedFriendship.DeletedAt = null;
            deletedFriendship.AcceptedAt = null;
            deletedFriendship.UpdatedAt = DateTime.UtcNow;
            _friendshipWrite.Update(deletedFriendship);
            await _friendshipWrite.SaveAsync();
            return Ok(new { success = true, message = "Arkadaşlık isteği gönderildi!" });
        }

        var friendship = new Friendship
        {
            RequesterId = userId.Value,
            AddresseeId = targetUserId,
            Status = FriendshipStatus.Pending
        };

        await _friendshipWrite.AddAsync(friendship);
        await _friendshipWrite.SaveAsync();

        return Ok(new { success = true, message = "Arkadaşlık isteği gönderildi!" });
    }

    [HttpPatch("friend-request/{requestId}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendship = await _friendshipRead.GetByIdAsync(requestId.ToString());
        if (friendship == null)
            return NotFound(new { success = false, message = "İstek bulunamadı." });

        if (friendship.AddresseeId != userId)
            return Unauthorized();

        if (friendship.Status != FriendshipStatus.Pending)
            return BadRequest(new { success = false, message = "Bu istek artık beklemede değil." });

        friendship.Status = FriendshipStatus.Accepted;
        friendship.AcceptedAt = DateTime.UtcNow;
        await _friendshipWrite.SaveAsync();

        return Ok(new { success = true, message = "Arkadaşlık isteği kabul edildi! 🎉" });
    }

    [HttpPatch("friend-request/{requestId}/reject")]
    public async Task<IActionResult> RejectFriendRequest(Guid requestId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendship = await _friendshipRead.GetByIdAsync(requestId.ToString());
        if (friendship == null)
            return NotFound(new { success = false, message = "İstek bulunamadı." });

        if (friendship.AddresseeId != userId)
            return Unauthorized();

        friendship.Status = FriendshipStatus.Rejected;
        await _friendshipWrite.SaveAsync();

        return Ok(new { success = true, message = "İstek reddedildi." });
    }

    [HttpPost("block/{targetUserId}")]
    public async Task<IActionResult> BlockUser(Guid targetUserId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (userId == targetUserId) return BadRequest(new { success = false, message = "Kendinizi engelleyemezsiniz." });

        var friendship = await _friendshipRead.GetAll(false)
            .FirstOrDefaultAsync(f =>
                (f.RequesterId == userId && f.AddresseeId == targetUserId) ||
                (f.RequesterId == targetUserId && f.AddresseeId == userId));

        if (friendship == null)
        {
            friendship = new Friendship
            {
                RequesterId = userId.Value,
                AddresseeId = targetUserId,
                Status = FriendshipStatus.Blocked
            };
            await _friendshipWrite.AddAsync(friendship);
        }
        else
        {
            friendship.Status = FriendshipStatus.Blocked;
            
            friendship.RequesterId = userId.Value;
            friendship.AddresseeId = targetUserId;
            _friendshipWrite.Update(friendship);
        }

        await _friendshipWrite.SaveAsync();
        return Ok(new { success = true, message = "Kullanıcı engellendi." });
    }

    [HttpPost("unblock/{targetUserId}")]
    public async Task<IActionResult> UnblockUser(Guid targetUserId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendship = await _friendshipRead.GetAll(false)
            .FirstOrDefaultAsync(f =>
                f.RequesterId == userId && f.AddresseeId == targetUserId && f.Status == FriendshipStatus.Blocked);

        if (friendship == null)
            return NotFound(new { success = false, message = "Engellenmiş kullanıcı bulunamadı." });

        _friendshipWrite.Remove(friendship);
        await _friendshipWrite.SaveAsync();

        return Ok(new { success = true, message = "Engelleme kaldırıldı." });
    }

    [HttpGet("blocked-users")]
    public async Task<IActionResult> GetBlockedUsers()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var blocked = await _friendshipRead.GetAll(false)
            .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Blocked)
            .Include(f => f.Addressee)
            .Select(f => new
            {
                id = f.Addressee.Id,
                name = f.Addressee.Name,
                surname = f.Addressee.Surname,
                userName = f.Addressee.UserName,
                avatarId = f.Addressee.AvatarId
            })
            .ToListAsync();

        return Ok(new { success = true, blockedUsers = blocked });
    }

    [HttpDelete("friends/{friendshipId}")]
    public async Task<IActionResult> RemoveFriend(Guid friendshipId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendship = await _friendshipRead.GetByIdAsync(friendshipId.ToString());
        if (friendship == null)
            return NotFound(new { success = false, message = "Arkadaşlık bulunamadı." });

        if (friendship.RequesterId != userId && friendship.AddresseeId != userId)
            return Unauthorized();

        _friendshipWrite.Remove(friendship);
        await _friendshipWrite.SaveAsync();

        return Ok(new { success = true, message = "Arkadaşlık sonlandırıldı." });
    }

    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return BadRequest(new { success = false, message = "En az 2 karakter girin." });

        var users = await _userRead.GetAll(false)
            .Where(u => u.Id != userId &&
                   (u.UserName.Contains(query) || u.Name.Contains(query) || u.Surname.Contains(query)))
            .Take(10)
            .Select(u => new
            {
                id = u.Id,
                name = u.Name,
                surname = u.Surname,
                userName = u.UserName,
                level = u.GeneralLevel,
                totalXP = u.TotalXP,
                avatarId = u.AvatarId
            })
            .ToListAsync();

        return Ok(new { success = true, users });
    }

    
    [HttpPost("kudos")]
    public async Task<IActionResult> GiveKudos([FromBody] GiveKudosCommandRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        request.SenderId = userId.Value;

        var response = await _mediator.Send(request);
        if (!response.Success) return BadRequest(response);

        return Ok(response);
    }

    
    
    

    
    [HttpGet("challenges")]
    public async Task<IActionResult> GetChallenges([FromQuery] string status = "active")
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var challenges = await _mediator.Send(new GetActiveChallengesQueryRequest
        {
            UserId = userId.Value,
            StatusFilter = status
        });

        return Ok(new { success = true, challenges });
    }

    
    [HttpPost("challenges/invite")]
    public async Task<IActionResult> SendChallengeInvite([FromBody] SendChallengeInviteCommandRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        request.ChallengerId = userId.Value;

        try
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    
    [HttpPatch("challenges/{challengeId}/respond")]
    public async Task<IActionResult> RespondToChallenge(Guid challengeId, [FromBody] ChallengeRespondBody body)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _mediator.Send(new RespondToChallengeCommandRequest
            {
                UserId = userId.Value,
                ChallengeId = challengeId,
                Accept = body.Accept,
                Message = body.Message
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    
    [HttpDelete("challenges/{challengeId}")]
    public async Task<IActionResult> CancelChallenge(Guid challengeId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var challenge = await _challengeRead
            .GetWhere(c => c.Id == challengeId && c.ChallengerId == userId, tracking: true)
            .FirstOrDefaultAsync();

        if (challenge == null)
            return NotFound(new { success = false, message = "Mücadele bulunamadı." });

        if (challenge.Status == ChallengeStatus.Completed)
            return BadRequest(new { success = false, message = "Tamamlanmış mücadele iptal edilemez." });

        challenge.Status = ChallengeStatus.Cancelled;
        _challengeWrite.Update(challenge);
        await _challengeWrite.SaveAsync();

        return Ok(new { success = true, message = "Mücadele iptal edildi." });
    }

    
    [HttpGet("challenges/{challengeId}")]
    public async Task<IActionResult> GetChallengeDetails(Guid challengeId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var challenge = await _challengeRead.Table.AsNoTracking()
            .Include(c => c.Challenger)
            .Include(c => c.Opponent)
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.TargetCategory)
            .Include(c => c.TargetAppTask)
            .FirstOrDefaultAsync(c => c.Id == challengeId);

        if (challenge == null)
            return NotFound(new { success = false, message = "Mücadele bulunamadı." });

        var participants = challenge.Participants
            .OrderByDescending(p => p.CurrentProgress)
            .Select(p => new
            {
                userId = p.UserId,
                userName = p.User.UserName,
                fullName = p.User.FullName,
                avatarId = p.User.AvatarId,
                score = p.CurrentProgress,
                isWinner = challenge.WinnerId == p.UserId
            });

        return Ok(new
        {
            success = true,
            challenge = new
            {
                challenge.Id,
                challenge.Title,
                challenge.Description,
                challenge.Status,
                challenge.ChallengeType,
                challenge.StartDate,
                challenge.EndDate,
                challenge.TargetValue,
                targetCategory = challenge.TargetCategory?.Name,
                targetTask = challenge.TargetAppTask?.Title,
                challengerProgress = challenge.ChallengerProgress,
                opponentProgress = challenge.OpponentProgress,
                winnerId = challenge.WinnerId,
                participants
            }
        });
    }

    
    
    

    
    [HttpPost("share-task")]
    public async Task<IActionResult> ShareTask([FromBody] ShareTaskBody body)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var user = await _userRead.GetByIdAsync(userId.Value.ToString());
        if (user == null) return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

        if (string.IsNullOrWhiteSpace(body.RecipientEmail))
            return BadRequest(new { success = false, message = "Geçerli bir e-posta adresi giriniz." });

        var task = await _appTaskRead.GetSingleAsync(t => t.Id == body.TaskId || t.Title == body.TaskTitle);
        if (task == null) return NotFound(new { success = false, message = "Paylaşılmak istenen görev sistemde bulunamadı." });

        var recipientUser = await _userRead.GetAll(false).FirstOrDefaultAsync(u => u.Email.ToLower() == body.RecipientEmail.ToLower());

        string frontendUrl = Request.Headers["Origin"].ToString();
        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            frontendUrl = "http://localhost:5173";
        }

        string actionLink;
        string inviteText;
        if (recipientUser != null)
        {
            actionLink = $"{frontendUrl.TrimEnd('/')}/login?acceptTask={task.Id}";
            inviteText = $@"<p>Bu görev <strong>{user.FullName}</strong> tarafından seninle paylaşıldı!</p>
                            <p>Daha fazlası için <a href='{actionLink}'>LifeQuest'e giriş yapın</a> ve gelişiminize hemen başlayın!</p>";

            
            var existingUserTask = await _userTaskRead.GetAll(false)
                .AnyAsync(ut => ut.AppUserId == recipientUser.Id && ut.AppTaskId == task.Id && ut.AssignedAt.Date == DateTime.UtcNow.Date);

            if (!existingUserTask)
            {
                var userTask = new UserTask
                {
                    AppUserId = recipientUser.Id,
                    AppTaskId = task.Id,
                    Status = AppTaskStatus.Suggested,
                    AssignedAt = DateTime.UtcNow
                };
                await _userTaskWrite.AddAsync(userTask);
                await _userTaskWrite.SaveAsync();
            }
        }
        else
        {
            actionLink = $"{frontendUrl.TrimEnd('/')}/register?acceptTask={task.Id}";
            inviteText = $@"<p>Bu heyecan verici görev <strong>{user.FullName}</strong> tarafından seninle paylaşıldı!</p>
                            <p>Hemen <a href='{actionLink}'>LifeQuest'e kaydolun</a> ve gelişiminize hemen başlayın!</p>";
        }

        var subject = $"LifeQuest Görev Paylaşımı: {task.Title}";
        var htmlMessage = $@"
            <h2>Merhaba!</h2>
            {inviteText}
            <hr />
            <h3>GÖREV DETAYI:</h3>
            <p><strong>Başlık:</strong> {task.Title}</p>
            <p><strong>Açıklama:</strong> {task.Description}</p>
            <p><strong>Zorluk Seviyesi:</strong> {task.Difficulty}</p>
            <p><strong>Kazanılacak XP:</strong> {task.BaseXP} XP</p>
            {(!string.IsNullOrEmpty(body.Message) ? $"<p><strong>Kullanıcı Mesajı:</strong> {System.Text.Encodings.Web.HtmlEncoder.Default.Encode(body.Message)}</p>" : "")}
        ";

        await _emailService.SendEmailAsync(body.RecipientEmail, subject, htmlMessage);

        return Ok(new
        {
            success = true,
            message = $"{body.RecipientEmail} adresine '{task.Title}' görevi başarıyla e-posta ile paylaşıldı! ✉️"
        });
    }

    private Guid? GetUserId()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return null;
        return Guid.Parse(userIdString);
    }
}

public class CreateChallengeBody
{
    public Guid OpponentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public int TargetTaskCount { get; set; } = 10;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BonusXP { get; set; } = 200;
}

public class ChallengeRespondBody
{
    public bool Accept { get; set; }
    public string? Message { get; set; }
}

public class ShareTaskBody
{
    public Guid? TaskId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string? Message { get; set; }
}
