using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class AppTask : BaseEntity
{
    public string Title { get; set; } =string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    public int BaseXP { get; set; }

    public DifficultyLevel Difficulty { get; set; }
    public bool IsDaily { get; set; }
    public bool IsActive { get; set; } = true;

    // Doğrulama gereksinimleri
    public VerificationType VerificationType { get; set; } = VerificationType.None;
    public int MinReflectionWords { get; set; } = 0;
    public int MinTimerMinutes { get; set; } = 0;
    public string? VerificationHint { get; set; }
    public string? ExpectedImageCategory { get; set; }
}