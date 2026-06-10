using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LifeQuestAPI.API.Controllers;

[Route("api/recommendation")]
[ApiController]
[Authorize(Roles = "Admin")] 
public class RecommendationController : ControllerBase
{
    private readonly ITaskRecommendationService _recommendationService;
    private readonly IUserTaskInteractionReadRepository _interactionReadRepository;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(
        ITaskRecommendationService recommendationService,
        IUserTaskInteractionReadRepository interactionReadRepository,
        ILogger<RecommendationController> logger)
    {
        _recommendationService = recommendationService;
        _interactionReadRepository = interactionReadRepository;
        _logger = logger;
    }

    [HttpPost("train")]
    public async Task<IActionResult> TrainModel()
    {
        _logger.LogInformation("ML.NET model eğitimi API üzerinden tetiklendi.");
        
        var allInteractions = await _interactionReadRepository.GetAll(false).ToListAsync();
        
        if (allInteractions.Count < 50)
        {
            return BadRequest(new { success = false, message = $"Yetersiz etkileşim verisi. En az 50 kayıt gerekiyor. Mevcut sayı: {allInteractions.Count}" });
        }

        var success = await _recommendationService.TrainModelAsync(allInteractions);

        if (success)
        {
            return Ok(new { success = true, message = $"Model başarıyla eğitildi. Eğitim veri sayısı: {allInteractions.Count}" });
        }
        else
        {
            return StatusCode(500, new { success = false, message = "Model eğitimi sırasında bir hata oluştu." });
        }
    }

    [HttpGet("status")]
    public IActionResult GetModelStatus()
    {
        return Ok(new 
        { 
            isReady = _recommendationService.IsModelReady,
            message = _recommendationService.IsModelReady 
                ? "ML.NET görev öneri modeli aktif ve hazır." 
                : "Model henüz hazır değil. Eğitim gerekli (en az 50 etkileşim kaydı sonrasında '/api/recommendation/train' tetiklenmeli)." 
        });
    }
}
