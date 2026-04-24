using LifeQuestAPI.Domain.Entities;

namespace LifeQuestAPI.Application.Repositories;

public interface IChatBotMessageReadRepository : IReadRepository<ChatBotMessage> { }
public interface IChatBotMessageWriteRepository : IWriteRepository<ChatBotMessage> { }
