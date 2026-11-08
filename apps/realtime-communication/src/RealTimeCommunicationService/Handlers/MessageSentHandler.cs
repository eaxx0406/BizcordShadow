using ChatService.Contracts.Events;
using RealTimeCommunicationService.Repositories;

namespace RealTimeCommunicationService.Handlers
{
    // Den ene "medarbejder", der får brevet, når ChatService melder, at en besked er sendt.
    // Ved den rigtige gruppe ville det være her, beskeden blev pushet ud til brugerne (fx via SignalR).
    // Vores dummy noterer bare i sin database, at den kom frem.
    public class MessageSentHandler
    {
        private readonly ReceivedMessageRepository _repository;

        public MessageSentHandler(ReceivedMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(MessageSent message)
        {
            await _repository.AddAsync(message);
        }
    }
}
