using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeQuestAPI.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IWeeklyReportReadRepository _reportReadRepository;

    public ReportsController(IWeeklyReportReadRepository reportReadRepository)
    {
        _reportReadRepository = reportReadRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyReports()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

        var userId = Guid.Parse(userIdString);

        var reports = await _reportReadRepository.Table
            .Where(r => r.AppUserId == userId)
            .OrderByDescending(r => r.WeekEndDate)
            .Take(10)
            .ToListAsync();

        return Ok(new { success = true, reports });
    }
}
