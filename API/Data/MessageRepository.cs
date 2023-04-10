using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {

        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddMessage(Message message) => _context.Messages.Add(message);

        public void RemoveMessage(Message message) => _context.Messages.Remove(message);

        public async Task<Message> GetMessageAsync(int id) => await _context.Messages.FindAsync(id);

        public async Task<bool> SaveAllAsync() => await _context.SaveChangesAsync() > 0;

        public async Task<PagedList<MessageDto>> GetMessageForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                 .OrderByDescending(x => x.MessageSent)
                 .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientName == messageParams.Username && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderName == messageParams.Username && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientName == messageParams.Username && u.RecipientDeleted == false && u.DateRead == null),
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                    m => m.RecipientName == currentUserName && 
                    m.RecipientDeleted == false &&
                    m.SenderName == recipientUserName || 
                    m.RecipientName == recipientUserName && 
                    m.SenderDeleted == false &&
                    m.SenderName == currentUserName
                )
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();

            var unreadMessage = messages.Where(m => m.DateRead == null && m.RecipientName == currentUserName).ToList();

            if (unreadMessage.Any())
            {
                foreach (var message in unreadMessage)
                {
                    message.DateRead = DateTime.UtcNow; 
                }

                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }
       
    }
}
