namespace Innowise.Application.Interfaces;

public interface IStoreTransactionFactory
{
    Task<IStoreTransaction> BeginAsync(CancellationToken cancellationToken = default);
}
