using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.SendMessage;
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
