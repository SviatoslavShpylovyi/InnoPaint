using Microsoft.EntityFrameworkCore;

using Innowise.Application.Interfaces;

namespace Innowise.Infrastructure.Data;

public sealed class StoreTransactionFactory : IStoreTransactionFactory
{
    private readonly InnoPaintDbContext _context;

    public StoreTransactionFactory(InnoPaintDbContext context)
    {
        _context = context;
    }

    public async Task<IStoreTransaction> BeginAsync(CancellationToken cancellationToken = default)
    {
        return new StoreTransaction(await _context.Database.BeginTransactionAsync(cancellationToken));
    }
}
