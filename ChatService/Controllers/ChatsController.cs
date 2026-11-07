using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.AddParticipant;
using ChatService.ServiceLayer.CreateChat;
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

        // POST api/chats 
        [HttpPost] //skal vi kalde den noget med newchat? eller createchat? ocelot skal i såfald opdateres
        public async Task<ActionResult<ChatDto>> Create(CreateChatRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Chat name cannot be empty.");
            }

            //MediatR will handle the command and return the created chat
            Chat chat = await _mediator.Send(new CreateChatCommand(request.Name, request.CreatedByUserId), cancellationToken);

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

        //// GET api/chats/{id}
        //[HttpGet("{id:guid}")]
        //public ActionResult<ChatDto> GetById(Guid id)
        //{
           
        //}
    }
}
