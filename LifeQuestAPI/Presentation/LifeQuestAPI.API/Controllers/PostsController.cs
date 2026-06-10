using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LifeQuestAPI.API.Controllers;

[Route("api/posts")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostReadRepository _postRead;
    private readonly IPostWriteRepository _postWrite;
    private readonly IPostTagWriteRepository _postTagWrite;
    private readonly IAppUserReadRepository _userRead;
    private readonly IFriendshipReadRepository _friendshipRead;
    private readonly IUserTaskReadRepository _userTaskRead;
    private readonly IPostLikeReadRepository _postLikeRead;
    private readonly IPostLikeWriteRepository _postLikeWrite;
    private readonly IPostCommentReadRepository _postCommentRead;
    private readonly IPostCommentWriteRepository _postCommentWrite;
    private readonly IWebHostEnvironment _env;

    public PostsController(
        IPostReadRepository postRead,
        IPostWriteRepository postWrite,
        IPostTagWriteRepository postTagWrite,
        IAppUserReadRepository userRead,
        IFriendshipReadRepository friendshipRead,
        IUserTaskReadRepository userTaskRead,
        IPostLikeReadRepository postLikeRead,
        IPostLikeWriteRepository postLikeWrite,
        IPostCommentReadRepository postCommentRead,
        IPostCommentWriteRepository postCommentWrite,
        IWebHostEnvironment env)
    {
        _postRead = postRead;
        _postWrite = postWrite;
        _postTagWrite = postTagWrite;
        _userRead = userRead;
        _friendshipRead = friendshipRead;
        _userTaskRead = userTaskRead;
        _postLikeRead = postLikeRead;
        _postLikeWrite = postLikeWrite;
        _postCommentRead = postCommentRead;
        _postCommentWrite = postCommentWrite;
        _env = env;
    }

    
    
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        
        var friendIds = await _friendshipRead.GetAll(false)
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId)
                        && f.Status == Domain.Enums.FriendshipStatus.Accepted)
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync();

        
        friendIds.Add(userId.Value);

        var posts = await _postRead.GetAll(false)
            .Where(p => friendIds.Contains(p.AppUserId))
            .Include(p => p.AppUser)
            .Include(p => p.Tags)
                .ThenInclude(t => t.AppTask)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                id = p.Id,
                content = p.Content,
                imageUrl = p.ImageUrl,
                createdAt = p.CreatedAt,
                author = new
                {
                    id = p.AppUser.Id,
                    userName = p.AppUser.UserName,
                    fullName = p.AppUser.FullName,
                    avatarId = p.AppUser.AvatarId,
                    level = p.AppUser.GeneralLevel
                },
                tags = p.Tags.Select(t => new
                {
                    id = t.Id,
                    taskId = t.AppTaskId,
                    userTaskId = t.UserTaskId, 
                    taskTitle = t.AppTask.Title,
                    x = t.CoordinateX,
                    y = t.CoordinateY
                }),
                likeCount = p.Likes.Count,
                commentCount = p.Comments.Count(),
                isLikedByMe = p.Likes.Any(l => l.AppUserId == userId)
            })
            .ToListAsync();

        return Ok(new { success = true, posts });
    }

    
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostDto request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content) && request.Image == null)
            return BadRequest(new { success = false, message = "Gönderi metni veya görsel zorunludur." });

        string? imageUrl = null;

        
        if (request.Image != null && request.Image.Length > 0)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(request.Image.ContentType.ToLower()))
                return BadRequest(new { success = false, message = "Sadece JPEG, PNG, WEBP veya GIF yükleyebilirsiniz." });

            if (request.Image.Length > 10 * 1024 * 1024) 
                return BadRequest(new { success = false, message = "Görsel boyutu 10MB'yi geçemez." });

            var wwwroot = _env.WebRootPath;
            if (string.IsNullOrEmpty(wwwroot))
            {
                wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }
            var uploadsFolder = Path.Combine(wwwroot, "uploads", "posts");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(request.Image.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await request.Image.CopyToAsync(fileStream);

            imageUrl = $"/uploads/posts/{uniqueFileName}";
        }

        
        if (string.IsNullOrWhiteSpace(request.TagsJson))
            return BadRequest(new { success = false, message = "Bu paylaşımı yapabilmek için en az bir tamamlanmış görev etiketlemelisiniz." });

        List<TagDto>? tags = null;
        try
        {
            tags = JsonSerializer.Deserialize<List<TagDto>>(request.TagsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch 
        { 
            return BadRequest(new { success = false, message = "Etiket verisi geçersiz." });
        }

        if (tags == null || tags.Count == 0)
            return BadRequest(new { success = false, message = "En az bir görev etiketlemelisiniz." });

        
        var taskIds = tags.Select(t => t.TaskId).ToList();
        
        
        Console.WriteLine($"[SocialFeed] User: {userId}, Received TaskIds: {string.Join(", ", taskIds)}");

        var validUserTasks = await _userTaskRead.GetAll(false)
            .Where(ut => ut.AppUserId == userId && 
                         (ut.CompletedAt.HasValue || ut.Status == Domain.Enums.AppTaskStatus.Completed) &&
                         (taskIds.Contains(ut.Id) || taskIds.Contains(ut.AppTaskId)))
            .ToListAsync();

        if (validUserTasks.Count == 0)
        {
            return BadRequest(new 
            { 
                success = false, 
                message = "Etiketlediğiniz görev(ler) henüz tamamlanmamış veya size ait değil.",
                error_detail = "Seçilen görev(ler) sistemde tamamlanmış olarak bulunamadı. Lütfen görevi bitirdiğinizden emin olun.",
                debug_info = $"Received {taskIds.Count} IDs, found 0 valid tasks for user {userId}."
            });
        }

        
        var post = new Post
        {
            AppUserId = userId.Value,
            Content = request.Content?.Trim(),
            ImageUrl = imageUrl
        };

        await _postWrite.AddAsync(post);
        await _postWrite.SaveAsync();

        
        var postTags = new List<PostTag>();
        foreach (var ut in validUserTasks)
        {
            if (!postTags.Any(pt => pt.AppTaskId == ut.AppTaskId))
            {
                postTags.Add(new PostTag
                {
                    PostId = post.Id,
                    AppTaskId = ut.AppTaskId,
                    UserTaskId = ut.Id, 
                    CoordinateX = 0,
                    CoordinateY = 0
                });
            }
        }

        await _postTagWrite.AddRangeAsync(postTags);
        await _postTagWrite.SaveAsync();

        return Ok(new { success = true, message = "Gönderi başarıyla paylaşıldı! 🎉", postId = post.Id });
    }

    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var post = await _postRead.GetByIdAsync(id.ToString());
        if (post == null) return NotFound(new { success = false, message = "Gönderi bulunamadı." });
        if (post.AppUserId != userId) return Forbid();

        
        if (!string.IsNullOrEmpty(post.ImageUrl))
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }

        _postWrite.Remove(post);
        await _postWrite.SaveAsync();

        return Ok(new { success = true, message = "Gönderi silindi." });
    }

    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var post = await _postRead.GetAll(false)
            .Where(p => p.Id == id)
            .Include(p => p.AppUser)
            .Include(p => p.Tags)
                .ThenInclude(t => t.AppTask)
            .Include(p => p.Likes)
            .Select(p => new
            {
                id = p.Id,
                content = p.Content,
                imageUrl = p.ImageUrl,
                createdAt = p.CreatedAt,
                author = new
                {
                    id = p.AppUser.Id,
                    userName = p.AppUser.UserName,
                    fullName = p.AppUser.FullName,
                    avatarId = p.AppUser.AvatarId,
                    level = p.AppUser.GeneralLevel
                },
                tags = p.Tags.Select(t => new
                {
                    id = t.Id,
                    taskId = t.AppTaskId,
                    userTaskId = t.UserTaskId,
                    taskTitle = t.AppTask.Title,
                    x = t.CoordinateX,
                    y = t.CoordinateY
                }),
                likeCount = p.Likes.Count,
                commentCount = p.Comments.Count(),
                isLikedByMe = p.Likes.Any(l => l.AppUserId == userId)
            })
            .FirstOrDefaultAsync();

        if (post == null) return NotFound(new { success = false, message = "Gönderi bulunamadı." });
        return Ok(new { success = true, post });
    }

    
    [HttpPut("{id}")]
    public async Task<IActionResult> EditPost(Guid id, [FromBody] EditPostDto request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var post = await _postRead.GetByIdAsync(id.ToString());
        if (post == null) return NotFound(new { success = false, message = "Gönderi bulunamadı." });
        if (post.AppUserId != userId) return Forbid();

        post.Content = request.Content?.Trim();
        post.UpdatedAt = DateTime.UtcNow;

        _postWrite.Update(post);
        await _postWrite.SaveAsync();

        return Ok(new { success = true, message = "Gönderi güncellendi." });
    }

    
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikePost(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existingLike = await _postLikeRead.GetAll(false)
            .FirstOrDefaultAsync(l => l.PostId == id && l.AppUserId == userId);

        if (existingLike != null)
            return BadRequest(new { success = false, message = "Zaten beğendiniz." });

        await _postLikeWrite.AddAsync(new PostLike { PostId = id, AppUserId = userId.Value });
        await _postLikeWrite.SaveAsync();

        return Ok(new { success = true });
    }

    
    [HttpDelete("{id}/like")]
    public async Task<IActionResult> UnlikePost(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var existingLike = await _postLikeRead.GetAll(false)
            .FirstOrDefaultAsync(l => l.PostId == id && l.AppUserId == userId);

        if (existingLike == null)
            return NotFound();

        _postLikeWrite.Remove(existingLike);
        await _postLikeWrite.SaveAsync();

        return Ok(new { success = true });
    }

    
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddCommentDto request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(new { success = false, message = "Yorum içeriği boş olamaz." });

        var comment = new PostComment
        {
            PostId = id,
            AppUserId = userId.Value,
            Content = request.Content.Trim()
        };

        await _postCommentWrite.AddAsync(comment);
        await _postCommentWrite.SaveAsync();

        return Ok(new { success = true, commentId = comment.Id });
    }

    
    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(Guid id)
    {
        var comments = await _postCommentRead.GetAll(false)
            .Where(c => c.PostId == id)
            .Include(c => c.AppUser)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                id = c.Id,
                content = c.Content,
                createdAt = c.CreatedAt,
                author = new
                {
                    id = c.AppUser.Id,
                    userName = c.AppUser.UserName,
                    fullName = c.AppUser.FullName,
                    avatarId = c.AppUser.AvatarId
                }
            })
            .ToListAsync();

        return Ok(new { success = true, comments });
    }

    
    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var comment = await _postCommentRead.GetByIdAsync(commentId.ToString());
        if (comment == null) return NotFound(new { success = false, message = "Yorum bulunamadı." });
        if (comment.AppUserId != userId) return Forbid();

        _postCommentWrite.Remove(comment);
        await _postCommentWrite.SaveAsync();

        return Ok(new { success = true, message = "Yorum silindi." });
    }

    private Guid? GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(value) ? null : Guid.Parse(value);
    }
}



public class CreatePostDto
{
    public string? Content { get; set; }
    public IFormFile? Image { get; set; }
    
    public string? TagsJson { get; set; }
}

public class EditPostDto
{
    public string Content { get; set; } = default!;
}

public class TagDto
{
    public Guid TaskId { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
}

public class AddCommentDto
{
    public string Content { get; set; } = default!;
}
