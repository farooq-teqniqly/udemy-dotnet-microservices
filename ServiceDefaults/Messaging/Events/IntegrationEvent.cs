namespace ServiceDefaults.Messaging.Events
{
  public abstract record IntegrationEvent
  {
    public Guid EventId => Guid.NewGuid();
    public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;

    public string EventType => GetType().AssemblyQualifiedName ?? "IntegrationEvent";
  }
}
