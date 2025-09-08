namespace ServiceDefaults.Messaging.Events
{
  public abstract record IntegrationEvent
  {
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public string EventType { get; init; }

    protected IntegrationEvent()
    {
      EventType = GetType().AssemblyQualifiedName ?? "IntegrationEvent";
    }
  }
}
