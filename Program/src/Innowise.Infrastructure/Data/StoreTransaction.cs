using Microsoft.EntityFrameworkCore.Storage;

using Innowise.Application.Interfaces;

namespace Innowise.Infrastructure.Data;

internal sealed class StoreTransaction : IStoreTransaction
{
    private readonly IDbContextTransaction _transaction;

    public StoreTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
