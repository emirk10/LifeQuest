using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LifeQuestAPI.Application.Abstractions.AI;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Domain.Entities.Identity;
using LifeQuestAPI.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace LifeQuestAPI.Infrastructure.Services.AI;

public class TaskRecommendationService : ITaskRecommendationService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<TaskRecommendationService> _logger;
    private ITransformer? _model;
    
    // Uygulama kök dizininde Models klasörü
    private readonly string _modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "task_recommendation_model.zip");

    public bool IsModelReady => _model != null;

    public TaskRecommendationService(ILogger<TaskRecommendationService> logger)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 42);
        LoadModel();
    }

    private void LoadModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var schema);
                _logger.LogInformation("ML.NET görev öneri modeli başarıyla yüklendi.");
            }
            else
            {
                _logger.LogWarning("ML.NET görev öneri modeli bulunamadı: {ModelPath}. Model eğitilene kadar kural tabanlı öneri kullanılacak.", _modelPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML.NET görev öneri modeli yüklenirken hata oluştu.");
        }
    }

    public async Task<List<Guid>> GetRecommendedTasksAsync(AppUser user, List<AppTask> availableTasks, int count = 3)
    {
        if (availableTasks == null || !availableTasks.Any())
            return new List<Guid>();

        // Eğer model yoksa Kural Tabanlı Fallback kullan
        if (!IsModelReady)
        {
            return GetRuleBasedRecommendations(user, availableTasks, count);
        }

        try 
        {
            var predEngine = _mlContext.Model.CreatePredictionEngine<TaskInteractionData, TaskPrediction>(_model);

            var scoredTasks = availableTasks.Select(task =>
            {
                var userCatLevelObj = user.CategoryProgresses?.FirstOrDefault(c => c.CategoryId == task.CategoryId);
                float userCatLevel = userCatLevelObj != null ? userCatLevelObj.Level : 1f;

                var inputData = new TaskInteractionData
                {
                    UserLevel = (float)user.GeneralLevel,
                    UserCategoryLevel = userCatLevel,
                    TaskDifficulty = (float)task.Difficulty,
                    TaskBaseXP = task.BaseXP,
                    DayOfWeek = (float)DateTime.Now.DayOfWeek,
                    HourOfDay = DateTime.Now.Hour,
                    UserCompletedTaskCount = user.UserTasks?.Count(ut => ut.Status == AppTaskStatus.Completed) ?? 0,
                    UserStreakDays = 0 // Eğer streak prop'u varsa kullanılabilir, şu an sabit
                };

                var prediction = predEngine.Predict(inputData);

                return new 
                {
                    Task = task,
                    Score = prediction.Probability
                };
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => x.Task.Id)
            .ToList();

            return scoredTasks.Any() ? scoredTasks : GetRuleBasedRecommendations(user, availableTasks, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ML modeli ile görev tahmini yapılırken hata, fallback kullanılacak.");
            return GetRuleBasedRecommendations(user, availableTasks, count);
        }
    }

    private List<Guid> GetRuleBasedRecommendations(AppUser user, List<AppTask> availableTasks, int count)
    {
        return availableTasks.Where(task => 
        {
            if (task.MinLevel > user.GeneralLevel) return false;

            if (task.CategoryId != Guid.Empty && user.CategoryProgresses != null)
            {
                var userCatProgress = user.CategoryProgresses.FirstOrDefault(cp => cp.CategoryId == task.CategoryId);
                int userCatLevel = userCatProgress?.Level ?? 1;

                switch (task.Difficulty)
                {
                    case DifficultyLevel.Medium:
                        if (userCatLevel < 2) return false;
                        break;
                    case DifficultyLevel.Hard:
                        if (userCatLevel < 4) return false;
                        break;
                }
            }
            return true;
        })
        .OrderBy(x => Guid.NewGuid()) // Geçici kural: randomize et
        .Take(count)
        .Select(t => t.Id)
        .ToList();
    }

    public async Task<bool> TrainModelAsync(List<UserTaskInteraction> trainingData)
    {
        if (trainingData == null || trainingData.Count < 50)
        {
            _logger.LogWarning("Model eğitmek için yetersiz veri. En az 50 kayıt gerekli. Mevcut: {Count}", trainingData?.Count ?? 0);
            return false;
        }

        try
        {
            var dataMapped = trainingData.Select(x => new TaskInteractionData
            {
                UserLevel = x.UserLevel,
                UserCategoryLevel = x.UserCategoryLevel,
                TaskDifficulty = x.TaskDifficulty,
                TaskBaseXP = x.TaskBaseXP,
                DayOfWeek = x.DayOfWeek,
                HourOfDay = x.HourOfDay,
                UserCompletedTaskCount = x.UserCompletedTaskCount,
                UserStreakDays = x.UserStreakDays,
                WasCompleted = x.WasCompleted
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(dataMapped);

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(TaskInteractionData.UserLevel),
                    nameof(TaskInteractionData.UserCategoryLevel),
                    nameof(TaskInteractionData.TaskDifficulty),
                    nameof(TaskInteractionData.TaskBaseXP),
                    nameof(TaskInteractionData.DayOfWeek),
                    nameof(TaskInteractionData.HourOfDay),
                    nameof(TaskInteractionData.UserCompletedTaskCount),
                    nameof(TaskInteractionData.UserStreakDays))
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(labelColumnName: "WasCompleted", featureColumnName: "Features"));

            _model = pipeline.Fit(dataView);

            var dir = Path.GetDirectoryName(_modelPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
            _logger.LogInformation("ML.NET modeli başarıyla eğitildi ve {Path} konumuna kaydedildi.", _modelPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Model eğitilirken bir hata oluştu.");
            return false;
        }
    }

    // --- ML.NET Data Models ---
    public class TaskInteractionData
    {
        public float UserLevel { get; set; }
        public float UserCategoryLevel { get; set; }
        public float TaskDifficulty { get; set; }
        public float TaskBaseXP { get; set; }
        public float DayOfWeek { get; set; }
        public float HourOfDay { get; set; }
        public float UserCompletedTaskCount { get; set; }
        public float UserStreakDays { get; set; }
        public bool WasCompleted { get; set; }
    }

    public class TaskPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool WillComplete { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}
