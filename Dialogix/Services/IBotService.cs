using System.Threading.Tasks;

namespace Dialogix.Services
{
    public interface IBotService
    {
        Task<string?> GetBotResponseAsync(string userMessage);
    }
}
