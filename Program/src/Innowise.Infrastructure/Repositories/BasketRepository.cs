using Microsoft.EntityFrameworkCore;

using Innowise.Application.Interfaces;
using Innowise.Domain.Models;
using Innowise.Infrastructure.Data;

namespace Innowise.Infrastructure.Repositories;

public sealed class BasketRepository : IBasketRepository
{
    private readonly InnoPaintDbContext _context;

    public BasketRepository(InnoPaintDbContext context)
    {
        _context = context;
    }

    public async Task<Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Baskets
            .Include(basket => basket.Items)
            .FirstOrDefaultAsync(basket => basket.Id == id, cancellationToken);
    }

    public async Task AddAsync(Basket basket, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(basket);

        await _context.Baskets.AddAsync(basket, cancellationToken);
    }

    public void Delete(Basket basket)
    {
        ArgumentNullException.ThrowIfNull(basket);

        _context.Baskets.Remove(basket);
    }

    public async Task<bool> ExistsItemExceedingQuantityAsync(Guid paintId, int quantity, CancellationToken cancellationToken = default)
    {
        return await _context.BasketItems
            .AsNoTracking()
            .AnyAsync(item => item.PaintId == paintId && item.Quantity > quantity, cancellationToken);
    }

    public async Task RemovePaintFromAllAsync(Guid paintId, CancellationToken cancellationToken = default)
    {
        await _context.BasketItems
            .Where(item => item.PaintId == paintId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
