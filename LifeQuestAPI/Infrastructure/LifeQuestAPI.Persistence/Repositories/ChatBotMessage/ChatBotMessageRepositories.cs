using LifeQuestAPI.Application.Repositories;
using LifeQuestAPI.Domain.Entities;
using LifeQuestAPI.Persistence.Contexts;

namespace LifeQuestAPI.Persistence.Repositories;

public class ChatBotMessageReadRepository : ReadRepository<ChatBotMessage>, IChatBotMessageReadRepository
{
    public ChatBotMessageReadRepository(LifeQuestDbContext context) : base(context) { }
}

public class ChatBotMessageWriteRepository : WriteRepository<ChatBotMessage>, IChatBotMessageWriteRepository
{
    public ChatBotMessageWriteRepository(LifeQuestDbContext context) : base(context) { }
}
