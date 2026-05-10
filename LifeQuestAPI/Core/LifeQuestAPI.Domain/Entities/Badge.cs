using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities.Common;
using LifeQuestAPI.Domain.Enums;

namespace LifeQuestAPI.Domain.Entities;

public sealed class Badge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;

    public BadgeType Type { get; set; } = BadgeType.Level;
    public int RequiredValue { get; set; }
    public bool IsSecret { get; set; } = false;
    public string BadgeColor { get; set; } = "#3b82f6";
    public string Rarity { get; set; } = "Common";

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; } = default!;

    [Obsolete("Use RequiredValue with Type instead")]
    public int RequiredLevel { get; set; }
    [Obsolete("Use Type instead")]
    public int CriteriaType { get; set; }
}