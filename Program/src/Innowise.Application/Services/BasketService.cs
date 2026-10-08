using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Exceptions;
using Innowise.Application.Interfaces;
using Innowise.Application.Mappings;
using Innowise.Domain.Models;

namespace Innowise.Application.Services;

public sealed class BasketService : IBasketService
{
    private readonly IBasketRepository _basketRepository;
    private readonly IPaintRepository _paintRepository;
    private readonly IStoreTransactionFactory _transactionFactory;

    public BasketService(
        IBasketRepository basketRepository,
        IPaintRepository paintRepository,
        IStoreTransactionFactory transactionFactory)
    {
        _basketRepository = basketRepository;
        _paintRepository = paintRepository;
        _transactionFactory = transactionFactory;
    }

    public async Task<BasketResponse> CreateAsync(CancellationToken cancellationToken = default)
    {
        var basket = Basket.Create();

        await _basketRepository.AddAsync(basket, cancellationToken);
        await _basketRepository.SaveChangesAsync(cancellationToken);

        return await ToResponseAsync(basket, cancellationToken);
    }

    public async Task<BasketResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _transactionFactory.BeginAsync(cancellationToken);

        var basket = await GetRequiredAsync(id, cancellationToken);
        var response = await ToResponseAsync(basket, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return response;
    }

    public async Task<BasketResponse> SetItemQuantityAsync(Guid basketId, Guid paintId, UpdateBasketItemRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var transaction = await _transactionFactory.BeginAsync(cancellationToken);

        var paint = await LockRequiredPaintAsync(paintId, cancellationToken);
        var basket = await GetRequiredAsync(basketId, cancellationToken);

        EnsureStockCovers(paint, request.Quantity);

        basket.SetItemQuantity(paintId, request.Quantity);
        await _basketRepository.SaveChangesAsync(cancellationToken);

        var response = await ToResponseAsync(basket, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return response;
    }

    public async Task RemoveItemAsync(Guid basketId, Guid paintId, CancellationToken cancellationToken = default)
    {
        var basket = await GetRequiredAsync(basketId, cancellationToken);

        if (!basket.RemoveItem(paintId))
        {
            throw new BasketItemNotFoundException(basketId, paintId);
        }

        await _basketRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var basket = await GetRequiredAsync(id, cancellationToken);

        basket.Clear();
        await _basketRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var basket = await GetRequiredAsync(id, cancellationToken);

        _basketRepository.Delete(basket);
        await _basketRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Basket> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _basketRepository.GetByIdAsync(id, cancellationToken) ?? throw new BasketNotFoundException(id);
    }

    private async Task<Paint> LockRequiredPaintAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _paintRepository.GetByIdForUpdateAsync(id, cancellationToken) ?? throw new PaintNotFoundException(id);
    }

    private static void EnsureStockCovers(Paint paint, int quantity)
    {
        if (!paint.HasStockFor(quantity))
        {
            throw new PaintStockExceededException(paint.Id, quantity, paint.StockQuantity);
        }
    }

    private async Task<BasketResponse> ToResponseAsync(Basket basket, CancellationToken cancellationToken)
    {
        var paintIds = basket.Items.Select(item => item.PaintId).ToArray();
        var paints = await _paintRepository.GetByIdsAsync(paintIds, cancellationToken);

        return basket.ToResponse(paints.ToDictionary(paint => paint.Id));
    }
}
