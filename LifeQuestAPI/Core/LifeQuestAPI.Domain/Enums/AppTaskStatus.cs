using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeQuestAPI.Domain.Enums;

public enum AppTaskStatus
{
    Assigned = 0,
    Completed = 1,
    InProgress = 2,
    Failed = 3,
    Expired = 4
}