namespace Dialogix.Services.Interfaces
{
    public interface IChatAiService
    {
        Task<string> GetReplyAsync(string userMessage);
    }
}
