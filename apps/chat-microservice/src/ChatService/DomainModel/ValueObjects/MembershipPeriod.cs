namespace ChatService.DomainModel.ValueObjects
{
    // Value object: det tidsrum, hvor en participant er med i en chat.
    // Ingen identitet: to perioder med samme tidspunkter ER den samme periode (record giver værdi-lighed).
    // Immutable: en periode ændres ikke – når man forlader chatten, laves en NY, afsluttet periode (se End).
    public record MembershipPeriod
    {
        public DateTimeOffset JoinedAt { get; }

        // null = perioden er ikke slut endnu (participanten er stadig med)
        public DateTimeOffset? LeftAt { get; }

        public MembershipPeriod(DateTimeOffset joinedAt, DateTimeOffset? leftAt = null)
        {
            // Regel: man kan ikke forlade en chat, før man er kommet ind i den
            if (leftAt is not null && leftAt < joinedAt)
            {
                throw new ArgumentException("LeftAt kan ikke ligge før JoinedAt.");
            }

            JoinedAt = joinedAt;
            LeftAt = leftAt;
        }

        // Ét sted, der definerer "aktivt medlemskab" (i stedet for "LeftAt is null" spredt rundt i koden)
        public bool IsActive => LeftAt is null;

        // Returnerer en ny, afsluttet periode – den gamle forbliver uændret
        public MembershipPeriod End(DateTimeOffset leftAt)
        {
            // Regel: en periode kan kun afsluttes én gang
            if (!IsActive)
            {
                throw new InvalidOperationException("Medlemskabet er allerede afsluttet.");
            }

            return new MembershipPeriod(JoinedAt, leftAt);
        }
    }
}
