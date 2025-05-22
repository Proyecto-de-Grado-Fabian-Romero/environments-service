namespace EnvironmentsService.Src.Application.Interfaces;

public interface IAdminServiceAdapter
{
    Task RequestTourAsync(Guid environmentId, Guid ownerId);
}
