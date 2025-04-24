namespace EnvironmentsService.Src.Domain.Interfaces;

public interface ITypeRepository
{
    Task<Guid> GetIdByPublicKeyAsync(string publicKey);
}
