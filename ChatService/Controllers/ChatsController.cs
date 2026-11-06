using ChatService.DomainModel.Entities;
using ChatService.ServiceLayer.CreateChat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers
{

    public record CreateChatRequest(string Name, Guid CreatedByUserId);// SKal du være her??
    public record ChatDto(Guid Id, string Name);// SKal du være her?? 

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
        [HttpPost]
        public async Task<ActionResult<ChatDto>> Create(CreateChatRequest request, CancellationToken cancellationToken)
        {
            // Validate the request

            //MediatR will handle the command and return the created chat
            Chat chat = await _mediator.Send(new CreateChatCommand(request.Name, request.CreatedByUserId), cancellationToken);

            //Så skal vi vel marshal det til en DTO vi kan returnere til klienten
            return Ok(new ChatDto(chat.Id, chat.Name));
        }

        //// GET api/chats/{id}
        //[HttpGet("{id:guid}")]
        //public ActionResult<ChatDto> GetById(Guid id)
        //{
           
        //}
    }
}
