namespace MessageClient.Interfaces
{
    public interface IMessageClient
    {
        // Der er sket noget i vores service (fx har nogen sendt en besked i chatten).
        // Vi afleverer "brevet" til posthuset (RabbitMQ). Typen T afgør, hvilke postkasser det havner i.
        Task Publish<T>(T domainEventMessage);

        // En service vil have besked, når et event af typen T sker.
        // subscriptionId = navnet servicen selv skriver på sin postkasse (fx "notification").
        // handler = adressen på den "medarbejder" i servicen, der skal have brevet, når det kommer.
        Task Subscribe<T>(string subscriptionId, Func<T, Task> handler);
    }
}
