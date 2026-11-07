using System.Reflection.Metadata.Ecma335;
using MediatR;
using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;

namespace ChatService.ServiceLayer.CreateChat
{
    public class CreateChatHandler:IRequestHandler<CreateChatCommand, Chat>
    {
        private readonly IChatWriteRepository _chatWriteRepository;

        public CreateChatHandler(IChatWriteRepository chatWriteRepository)
        {
            _chatWriteRepository = chatWriteRepository;
        }

        public async Task<Chat> Handle(CreateChatCommand command, CancellationToken cancellationToken)
        {
            Chat newChat = new Chat(command.Name);
            newChat.AddParticipant(command.CreatedByUserId);

            await _chatWriteRepository.CreateAsync(newChat, cancellationToken);
            return newChat;
        }
    }
}
