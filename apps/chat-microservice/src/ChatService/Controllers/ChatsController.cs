using ChatService.Contracts;
using ChatService.Contracts.Validation;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.AddParticipant;
using ChatService.ServiceLayer.CreateChat;
using ChatService.ServiceLayer.GetChatById;
using ChatService.ServiceLayer.GetMyChats;
using ChatService.ServiceLayer.LeaveChat;
using ChatService.ServiceLayer.RenameChat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{
    // Controlleren er kun præsentation: den tager imod HTTP, sender en command/query til MediatR og vælger statuskode.
    // Validering af input sker med attributter ([Required], [NotEmptyGuid] osv.): [ApiController] svarer så selv 400
    // med ProblemDetails, før metoden overhovedet kaldes. Domænefejl bliver til 400/403/404/409 i DomainExceptionHandler.
    // Regler for statuskoder: se ADR 0020.
    [ApiController]
    [Route("api/[controller]")]
    public class ChatsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/chats?userId=...
        // userId sendes af klienten, indtil der findes login. Så forsvinder parameteren, og GET api/chats betyder "mine".
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ChatSummaryDto>>> GetMine([FromQuery, NotEmptyGuid] Guid userId, CancellationToken cancellationToken)
        {
            //MediatR will handle the query and return the user's chats (an empty list if there are none)
            IReadOnlyList<ChatSummaryDto> chats = await _mediator.Send(new GetMyChatsQuery(userId), cancellationToken);

            return Ok(chats);
        }

        // GET api/chats/{id}?userId=...
        // Kun medlemmer må se chatten, ellers 403 (ADR 0013). userId sendes af klienten, indtil der findes login.
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatDetailsDto>> GetById(Guid id, [FromQuery, NotEmptyGuid] Guid userId, CancellationToken cancellationToken)
        {
            //MediatR will handle the query and return the chat (ForbiddenException -> 403 if the user is not a member)
            ChatDetailsDto chat = await _mediator.Send(new GetChatByIdQuery(id, userId), cancellationToken);

            return Ok(chat);
        }

        // POST api/chats
        // 201 Created + Location-header, der peger på den nye chat (GET api/chats/{id})
        [HttpPost]
        public async Task<ActionResult<ChatDto>> Create(CreateChatRequest request, CancellationToken cancellationToken)
        {
            //MediatR will handle the command and return the created chat
            //(reglen "mindst ét medlem ud over opretteren" håndhæves af Chat.Create -> 400, ADR 0012)
            Chat chat = await _mediator.Send(new CreateChatCommand(request.Name, request.CreatedByUserId, request.ParticipantUserIds), cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = chat.Id }, new ChatDto(chat.Id, chat.Name));
        }

        // PUT api/chats/{chatId}
        // Omdøber chatten. Navnet er det eneste, der kan ændres, så PUT erstatter hele den redigerbare del. 204: intet at sende tilbage.
        [HttpPut("{chatId:guid}")]
        public async Task<ActionResult> Rename(Guid chatId, [FromBody] RenameChatRequest request, CancellationToken cancellationToken)
        {
            //MediatR will handle the command (ForbiddenException -> 403 if the user is not a member)
            await _mediator.Send(new RenameChatCommand(chatId, request.UserId, request.Name), cancellationToken);

            return NoContent();
        }

        // POST api/chats/{chatId}/participants
        // Tilføjer en deltager (ADR 0010). 204: medlemskabet har ingen egen URL, så der er intet at pege på eller sende tilbage.
        [HttpPost("{chatId:guid}/participants")]
        public async Task<ActionResult> AddParticipant(Guid chatId, [FromBody] AddParticipantRequest request, CancellationToken cancellationToken)
        {
            //MediatR will handle the command
            await _mediator.Send(new AddParticipantCommand(chatId, request.UserId), cancellationToken);

            return NoContent();
        }

        // DELETE api/chats/{chatId}/participants/{userId}
        // Brugeren forlader selv chatten. Medlemskabet afsluttes (left_at sættes), det slettes ikke (ADR 0019).
        // userId er den, der forlader – indtil der findes login. Så bliver det .../participants/me.
        [HttpDelete("{chatId:guid}/participants/{userId:guid}")]
        public async Task<ActionResult> Leave(Guid chatId, [NotEmptyGuid] Guid userId, CancellationToken cancellationToken)
        {
            //MediatR will handle the command (ForbiddenException -> 403 if the user is not a member, ADR 0013)
            await _mediator.Send(new LeaveChatCommand(chatId, userId), cancellationToken);

            //204: det lykkedes, og der er intet at sende tilbage
            return NoContent();
        }
    }
}
