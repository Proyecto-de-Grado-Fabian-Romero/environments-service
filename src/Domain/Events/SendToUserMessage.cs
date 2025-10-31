namespace EnvironmentsService.Src.Domain.Events;

public sealed class SendToUserMessage
{
    public Guid NotificationId { get; init; }

    public Guid UserPublicId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string Type { get; init; } = "Info";

    public string Channel { get; init; } = "Push";

    public Dictionary<string, string>? Metadata { get; init; }
}
