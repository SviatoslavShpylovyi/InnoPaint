using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;

namespace Innowise.Application.Interfaces;

public interface IBasketService
{
    Task<BasketResponse> CreateAsync(CancellationToken cancellationToken = default);
    Task<BasketResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BasketResponse> SetItemQuantityAsync(Guid basketId, Guid paintId, UpdateBasketItemRequest request, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid basketId, Guid paintId, CancellationToken cancellationToken = default);
    Task ClearAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
