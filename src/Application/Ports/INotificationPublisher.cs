using EnvironmentsService.Src.Domain.Events;

namespace EnvironmentsService.Src.Application.Ports;

public interface INotificationsPublisher
{
    void PublishSendToUser(SendToUserMessage msg);
}
