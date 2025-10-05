using Dialogix.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dialogix.Services
{
    public interface IChatRepository
    {
        Task<IEnumerable<ChatMessage>> GetAllMessagesAsync();
        Task<ChatMessage> GetMessageByIdAsync(int id);
        Task AddMessageAsync(ChatMessage message);
        Task UpdateMessageAsync(ChatMessage message);
        Task DeleteMessageAsync(int id);
    }
}
