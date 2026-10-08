using Innowise.Domain.Models;

namespace Innowise.Application.Interfaces;

public interface IBasketRepository
{
    Task<Basket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Basket basket, CancellationToken cancellationToken = default);
    void Delete(Basket basket);
    Task<bool> ExistsItemExceedingQuantityAsync(Guid paintId, int quantity, CancellationToken cancellationToken = default);
    Task RemovePaintFromAllAsync(Guid paintId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
