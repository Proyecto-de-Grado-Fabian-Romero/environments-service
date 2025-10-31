namespace EnvironmentsService.Src.Application.Interfaces;

public interface IAdminServiceAdapter
{
    Task RequestTourAsync(Guid environmentId, Guid ownerId);

    Task RequestOwnerIncomeAsync(Guid ownerId, Guid reservationId, decimal amount, string currency, long generatedAt);
}
