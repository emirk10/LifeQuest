using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Application.Abstractions.AI;

public interface ITaskRecommendationService
{
    
    
    
    Task<List<Guid>> GetRecommendedTasksAsync(AppUser user, List<AppTask> availableTasks, int count = 3);
    
    
    
    
    Task<bool> TrainModelAsync(List<UserTaskInteraction> trainingData);
    
    
    
    
    bool IsModelReady { get; }
}
