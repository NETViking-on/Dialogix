using Dialogix.Data;
using Dialogix.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dialogix.Services
{
    public class ChatRepository : IChatRepository
    {
        private readonly ChatDbContext _context;

        public ChatRepository(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatMessage>> GetAllMessagesAsync()
        {
            return await _context.ChatMessages
                                 .AsNoTracking()
                                 .OrderBy(m => m.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<ChatMessage> GetMessageByIdAsync(int id)
        {
            var message = await _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
                throw new KeyNotFoundException($"ChatMessage with Id {id} not found.");
            return message;
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _context.ChatMessages.FindAsync(id);
            if (message != null)
            {
                _context.ChatMessages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }
    }
}
