using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    // ChatService ejer selv sine repository-interfaces (se ADR 0009).
    public interface IChatWriteRepository
    {
        // Gemmer en ny chat inkl. dens participations i én transaktion.
        Task CreateAsync(Chat chat, CancellationToken cancellationToken);

        // Gemmer KUN én ny participation i en eksisterende chat (se ADR 0010).
        Task AddParticipationAsync(Participation participation, CancellationToken cancellationToken);

        // Gemmer at en participation er afsluttet (participanten har forladt chatten).
        Task LeaveParticipationAsync(Participation participation, CancellationToken cancellationToken);
    }
}
