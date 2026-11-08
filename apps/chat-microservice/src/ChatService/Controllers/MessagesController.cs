using ChatService.Contracts;
using ChatService.Contracts.Validation;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.DeleteMessage;
using ChatService.ServiceLayer.GetMessages;
using ChatService.ServiceLayer.SendMessage;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    // Beskeder er deres eget aggregate, derfor egen controller.
    // URL'en ligger under chatten, fordi en besked altid hører til én chat.
    // Validering og statuskoder følger samme regler som ChatsController (ADR 0020).
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
        public async Task<ActionResult<MessagePageDto>> Get(Guid chatId, [FromQuery, NotEmptyGuid] Guid userId, [FromQuery] Guid? before,
            [FromQuery, Range(1, 100)] int limit = 30, CancellationToken cancellationToken = default)
        {
            //MediatR will handle the query and return one page of messages, oldest first
            MessagePageDto page = await _mediator.Send(new GetMessagesQuery(chatId, userId, before, limit), cancellationToken);

            return Ok(page);
        }

        // POST api/chats/{chatId}/messages
        // 201 Created med den nye besked. Ingen Location-header: der findes ikke en GET for én enkelt besked.
        [HttpPost]
        public async Task<ActionResult<MessageDto>> Send(Guid chatId, [FromBody] SendMessageRequest request, CancellationToken cancellationToken)
        {
            //MediatR will handle the command and return the sent message
            Message message = await _mediator.Send(new SendMessageCommand(chatId, request.SenderUserId, request.Content), cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new MessageDto(message.Id, message.ChatId, message.Content, message.SentAt));
        }

        // DELETE api/chats/{chatId}/messages/{messageId}?userId=...
        // Afsenderen sletter sin besked (soft delete, ADR 0020). userId i query'en og ikke i en body,
        // fordi DELETE med body ikke er understøttet alle steder. 204 – også hvis den allerede var slettet (idempotent).
        [HttpDelete("{messageId:guid}")]
        public async Task<ActionResult> Delete(Guid chatId, Guid messageId, [FromQuery, NotEmptyGuid] Guid userId, CancellationToken cancellationToken)
        {
            //MediatR will handle the command (403 if not a member or not the sender, 404 if the message doesn't exist)
            await _mediator.Send(new DeleteMessageCommand(chatId, messageId, userId), cancellationToken);

            return NoContent();
        }
    }
}
