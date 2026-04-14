using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;

namespace LifeQuestAPI.Application.Abstractions.AI;

public interface ITaskRecommendationService
{
    /// <summary>
    /// Kullanıcı için ML.NET modelini kullanarak en uygun görevleri önerir.
    /// </summary>
    Task<List<Guid>> GetRecommendedTasksAsync(AppUser user, List<AppTask> availableTasks, int count = 3);
    
    /// <summary>
    /// Sağlanan eğitim verisi ile modeli eğitir veya yeniden eğitir.
    /// </summary>
    Task<bool> TrainModelAsync(List<UserTaskInteraction> trainingData);
    
    /// <summary>
    /// Servis hazır mı? Modelin yüklenip yüklenmediğini gösterir.
    /// </summary>
    bool IsModelReady { get; }
}
