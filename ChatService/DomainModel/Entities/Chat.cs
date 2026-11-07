using ChatService.DomainModel.Exceptions;

namespace ChatService.DomainModel.Entities
{
    // Aggregate root: en Chat ejer sine Participations.
    // Ændringer i deltagere sker KUN gennem metoder på Chat, så reglerne altid håndhæves ét sted.
    // Messages er IKKE en del af aggregatet (de er deres eget aggregate og refererer kun ChatId).
    public class Chat
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        private readonly List<Participation> _participations = new();
        public IReadOnlyCollection<Participation> Participations => _participations;

        private static int _maxParticipants = 50; 

        // Private: nye chats laves med Create, så de altid har medlemmer fra start.
        private Chat(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        // Opretter en ny chat med opretteren og mindst ét andet medlem (se ADR 0012).
        // Medlemmerne tilføjes gennem AddParticipant, så de samme regler gælder som ved tilføjelse senere (ADR 0010).
        public static Chat Create(string name, Guid creatorParticipantId, IEnumerable<Guid> memberParticipantIds)
        {
            Chat chat = new Chat(name);
            chat.AddParticipant(creatorParticipantId);

            foreach (Guid memberParticipantId in memberParticipantIds)
            {
                chat.AddParticipant(memberParticipantId);
            }

            // Regel: en chat kun med en selv giver ikke mening
            if (chat._participations.Count < 2)
            {
                throw new InvalidOperationException("En chat skal have mindst ét medlem ud over opretteren.");
            }

            return chat;
        }

        // Tom konstruktor, som kun Rehydrate må bruge (derfor private).
        private Chat() { }

        // Rehydrering: genopbygger en EKSISTERENDE chat ud fra data fra databasen.
        // Laver ikke et nyt id – bruger det, der står i databasen. Bruges kun af repository'et.
        public static Chat Rehydrate(Guid id, string name, IEnumerable<Participation> participations)
        {
            Chat chat = new Chat();
            chat.Id = id;
            chat.Name = name;
            chat._participations.AddRange(participations);
            return chat;
        }

        public Participation AddParticipant(Guid participantId)
        {
            // Regel: kun ét aktivt medlemskab per participant (samme regel som ux_participation_active i databasen)
            if (_participations.Any(p => p.ParticipantId == participantId && p.LeftAt is null))
            {
                throw new ConflictException("Participant er allerede med i chatten.");
            }

            //regel: Der er et maximum antal deltagere i en chat (se ADR 0010)
            if (_participations.Count >= _maxParticipants)
            {
                throw new ConflictException("Chatten har nået maksimalt antal deltagere.");
            }
            
            Participation participation = new Participation(Id, participantId);
            _participations.Add(participation);
            return participation;
        }
    }
}
