using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.GetMessages;
using ChatService.ServiceLayer.SendMessage;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    // Beskeder er deres eget aggregate, derfor egen controller.
    // URL'en ligger under chatten, fordi en besked altid hører til én chat.
    [ApiController]
    [Route("api/chats/{chatId:guid}/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MessagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/chats/{chatId}/messages?userId=...&before=...&limit=30
        // userId sendes af klienten, indtil der findes login. before = id på den ældste besked, man allerede har.
        [HttpGet]
        public async Task<ActionResult<MessagePageDto>> Get(Guid chatId, [FromQuery] Guid userId, [FromQuery] Guid? before,
            [FromQuery, Range(1, 100)] int limit = 30, CancellationToken cancellationToken = default)
        {
            // Validate the request ([Range] på limit giver automatisk 400 via [ApiController])
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            //MediatR will handle the query and return one page of messages, oldest first
            MessagePageDto page = await _mediator.Send(new GetMessagesQuery(chatId, userId, before, limit), cancellationToken);

            return Ok(page);
        }

        // POST api/chats/{chatId}/messages
        [HttpPost]
        public async Task<ActionResult<MessageDto>> Send(Guid chatId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request.SenderUserId == Guid.Empty)
            {
                return BadRequest("Sender user ID cannot be empty.");
            }

            //MediatR will handle the command and return the sent message
            Message message = await _mediator.Send(new SendMessageCommand(chatId, request.SenderUserId, request.Content), cancellationToken);

            return Ok(new MessageDto(message.Id, message.ChatId, message.Content, message.SentAt));
        }
    }
}
