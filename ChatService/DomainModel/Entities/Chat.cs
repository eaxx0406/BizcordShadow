namespace ChatService.DomainModel.Entities
{
    // Aggregate root: en Chat ejer sine Participations.
    // Ændringer i deltagere sker KUN gennem metoder på Chat, så reglerne altid håndhæves ét sted.
    // Messages er IKKE en del af aggregatet (de er deres eget aggregate og refererer kun ChatId).
    public class Chat
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Privat liste udefra kun læsbar: ingen kan kalde chat.Participations.Add(...) og omgå reglerne.
        private readonly List<Participation> _participations = new();
        public IReadOnlyCollection<Participation> Participations => _participations;

        //konstruktor der laver en chat
        public Chat(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public void AddParticipant(Guid participantId)
        {
            // Regel: kun ét aktivt medlemskab per participant (samme regel som ux_participation_active i databasen)
            if (_participations.Any(p => p.ParticipantId == participantId && p.LeftAt is null))
            {
                throw new InvalidOperationException("Participant er allerede med i chatten.");
            }

            _participations.Add(new Participation(Id, participantId));
        }
    }
}
