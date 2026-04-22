using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Enums;
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

    public SocialController(
        IFriendshipReadRepository friendshipRead,
        IFriendshipWriteRepository friendshipWrite,
        IChallengeReadRepository challengeRead,
        IChallengeWriteRepository challengeWrite,
        IAppUserReadRepository userRead,
        IAppUserWriteRepository userWrite)
    {
        _friendshipRead = friendshipRead;
        _friendshipWrite = friendshipWrite;
        _challengeRead = challengeRead;
        _challengeWrite = challengeWrite;
        _userRead = userRead;
        _userWrite = userWrite;
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
                userName = friend.UserName,
                level = friend.GeneralLevel,
                totalXP = friend.TotalXP,
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

        // Hedef kullanıcı var mı?
        var targetUser = await _userRead.GetByIdAsync(targetUserId.ToString());
        if (targetUser == null)
            return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

        // Zaten arkadaş veya istek var mı?
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
                totalXP = u.TotalXP
            })
            .ToListAsync();

        return Ok(new { success = true, users });
    }

    // ================================================================
    // MÜCADELELER
    // ================================================================

    [HttpGet("challenges")]
    public async Task<IActionResult> GetChallenges()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var challenges = await _challengeRead.GetAll(false)
            .Where(c => c.ChallengerId == userId || c.OpponentId == userId)
            .Include(c => c.Challenger)
            .Include(c => c.Opponent)
            .Include(c => c.Category)
            .OrderByDescending(c => c.StartDate)
            .Select(c => new
            {
                id = c.Id,
                title = c.Title,
                challengerName = c.Challenger.Name + " " + c.Challenger.Surname,
                opponentName = c.Opponent.Name + " " + c.Opponent.Surname,
                challengerId = c.ChallengerId,
                opponentId = c.OpponentId,
                categoryName = c.Category != null ? c.Category.Name : null,
                targetTaskCount = c.TargetTaskCount,
                challengerProgress = c.ChallengerProgress,
                opponentProgress = c.OpponentProgress,
                startDate = c.StartDate,
                endDate = c.EndDate,
                isActive = c.IsActive,
                winnerId = c.WinnerId,
                winnerBonusXP = c.WinnerBonusXP
            })
            .ToListAsync();

        return Ok(new { success = true, challenges });
    }

    [HttpPost("challenges")]
    public async Task<IActionResult> CreateChallenge([FromBody] CreateChallengeBody body)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        // Rakip kullanıcı var mı?
        var opponent = await _userRead.GetByIdAsync(body.OpponentId.ToString());
        if (opponent == null)
            return NotFound(new { success = false, message = "Rakip kullanıcı bulunamadı." });

        // Arkadaş mı?
        var areFriends = await _friendshipRead.GetAll(false)
            .AnyAsync(f =>
                ((f.RequesterId == userId && f.AddresseeId == body.OpponentId) ||
                 (f.RequesterId == body.OpponentId && f.AddresseeId == userId)) &&
                f.Status == FriendshipStatus.Accepted);

        if (!areFriends)
            return BadRequest(new { success = false, message = "Sadece arkadaşlarınıza mücadele daveti gönderebilirsiniz." });

        var challenge = new Challenge
        {
            ChallengerId = userId.Value,
            OpponentId = body.OpponentId,
            CategoryId = body.CategoryId,
            Title = body.Title,
            TargetTaskCount = body.TargetTaskCount,
            StartDate = body.StartDate,
            EndDate = body.EndDate,
            WinnerBonusXP = body.BonusXP > 0 ? body.BonusXP : 200,
            IsActive = true
        };

        await _challengeWrite.AddAsync(challenge);
        await _challengeWrite.SaveAsync();

        return Ok(new { success = true, message = "Mücadele daveti gönderildi! ⚔️", challengeId = challenge.Id });
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
