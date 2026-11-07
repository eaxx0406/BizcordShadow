using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.AddParticipant;
using ChatService.ServiceLayer.CreateChat;
using ChatService.ServiceLayer.GetChatById;
using ChatService.ServiceLayer.GetMyChats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{

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
        public async Task<ActionResult<IReadOnlyList<ChatSummaryDto>>> GetMine([FromQuery] Guid userId, CancellationToken cancellationToken)
        {
            // Validate the request
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            //MediatR will handle the query and return the user's chats (an empty list if there are none)
            IReadOnlyList<ChatSummaryDto> chats = await _mediator.Send(new GetMyChatsQuery(userId), cancellationToken);

            return Ok(chats);
        }

        // POST api/chats
        [HttpPost] //skal vi kalde den noget med newchat? eller createchat? ocelot skal i såfald opdateres
        public async Task<ActionResult<ChatDto>> Create(CreateChatRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Chat name cannot be empty.");
            }

            // A chat needs at least one member besides the creator (ADR 0012)
            if (request.ParticipantUserIds is null || !request.ParticipantUserIds.Any(id => id != request.CreatedByUserId))
            {
                return BadRequest("A chat needs at least one member besides the creator.");
            }

            //MediatR will handle the command and return the created chat
            Chat chat = await _mediator.Send(new CreateChatCommand(request.Name, request.CreatedByUserId, request.ParticipantUserIds), cancellationToken);

            //Så skal vi vel marshal det til en DTO vi kan returnere til klienten
            return Ok(new ChatDto(chat.Id, chat.Name));
        }

        [HttpPost("{chatId:guid}/participants")] //api/chats/{chatId}/participants måske
        public async Task<ActionResult> AddParticipant(Guid chatId, [FromBody] AddParticipantRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (request.UserId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            //MediatR will handle the command
            await _mediator.Send(new AddParticipantCommand(chatId, request.UserId), cancellationToken);

            return Ok();
        }

        // GET api/chats/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChatDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            //MediatR will handle the query and return the chat, or null if it does not exist
            ChatDetailsDto? chat = await _mediator.Send(new GetChatByIdQuery(id), cancellationToken);

            if (chat is null)
            {
                return NotFound();
            }

            return Ok(chat);
        }
    }
}
