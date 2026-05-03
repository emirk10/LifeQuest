using LifeQuestAPI.Application.Features.Social.Commands.SendChallengeInvite;
using LifeQuestAPI.Application.Features.Social.Commands.RespondToChallenge;
using LifeQuestAPI.Application.Features.Social.Queries.GetActiveChallenges;
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
    private readonly IMediator _mediator;

    public SocialController(
        IFriendshipReadRepository friendshipRead,
        IFriendshipWriteRepository friendshipWrite,
        IChallengeReadRepository challengeRead,
        IChallengeWriteRepository challengeWrite,
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite,
        IMediator mediator)
    {
        _friendshipRead = friendshipRead;
        _friendshipWrite = friendshipWrite;
        _challengeRead = challengeRead;
        _challengeWrite = challengeWrite;
        _userRead = userRead;
        _userWrite = userWrite;
        _mediator = mediator;
    }

    // ================================================================
    // ARKADAŞLIK
    // ================================================================

    [HttpGet("friends")]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var friendships = await _friendshipRead.GetAll(false)
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
            .Include(f => f.Requester)
            .Include(f => f.Addressee)
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

        return Ok(new { success = true, friends });
    }

    [HttpGet("friend-requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var requests = await _friendshipRead.GetAll(false)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
            .Include(f => f.Requester)
            .OrderByDescending(f => f.CreatedAt)
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

        return Ok(new { success = true, requests });
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
            var msg = existing.Status == FriendshipStatus.Accepted
                ? "Zaten arkadaşsınız."
                : "Zaten bir istek gönderilmiş veya mevcut.";
            return BadRequest(new { success = false, message = msg });
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

    // ================================================================
    // SP-05: MÜCADELELER — MediatR Handler'lar
    // ================================================================

    /// <summary>SP-05: Mücadele listesi (status: active | invited | completed | all)</summary>
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

    /// <summary>SP-05: Mücadele daveti gönder</summary>
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

    /// <summary>SP-05: Mücadele davetine cevap ver (kabul/red)</summary>
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

    /// <summary>SP-05: Mücadeleyi iptal et</summary>
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

    // ================================================================
    // YARDIMCI
    // ================================================================

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
