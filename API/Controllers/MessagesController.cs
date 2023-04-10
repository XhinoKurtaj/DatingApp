using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var uid = User.GetUserId();
            var sender = await _userRepository.GetUserByIdAsync(uid);

            if (sender == null) return NotFound();

            if (sender.UserName == createMessageDto.RecipienUsername.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipienUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderName = sender.UserName,
                RecipientName = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            var uid = User.GetUserId();
            var user = await _userRepository.GetUserByIdAsync(uid);

            messageParams.Username = user.UserName;

            var messages = await _messageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var uid = User.GetUserId();
            var user = await _userRepository.GetUserByIdAsync(uid);

            return Ok(await _messageRepository.GetMessageThread(user.UserName, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var uid = User.GetUserId();
            var user = await _userRepository.GetUserByIdAsync(uid);

            var message = await _messageRepository.GetMessageAsync(id);

            if(message.SenderName != user.UserName && message.RecipientName != user.UserName)
                return Unauthorized();

            if(message.SenderName == user.UserName ) message.SenderDeleted = true;
            if(message.RecipientName == user.UserName ) message.RecipientDeleted = true;

            if( message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.RemoveMessage(message);
            }

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");

        }

    }
}
