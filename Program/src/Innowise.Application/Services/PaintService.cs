using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Exceptions;
using Innowise.Application.Interfaces;
using Innowise.Application.Mappings;
using Innowise.Domain.Models;

namespace Innowise.Application.Services;

public sealed class PaintService : IPaintService
{
    private readonly IPaintRepository _paintRepository;
    private readonly IBasketRepository _basketRepository;
    private readonly IStoreTransactionFactory _transactionFactory;

    public PaintService(
        IPaintRepository paintRepository,
        IBasketRepository basketRepository,
        IStoreTransactionFactory transactionFactory)
    {
        _paintRepository = paintRepository;
        _basketRepository = basketRepository;
        _transactionFactory = transactionFactory;
    }

    public async Task<PaintPageResponse> GetAllAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using var transaction = await _transactionFactory.BeginAsync(cancellationToken);

        var totalCount = await _paintRepository.CountAsync(filter, cancellationToken);
        var paints = await _paintRepository.GetAllAsync(filter, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PaintPageResponse(paints.ToResponses(), totalCount, filter.Page, filter.PageSize);
    }

    public async Task<PaintLookupsResponse> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        var brands = await _paintRepository.GetBrandsAsync(cancellationToken);
        var colors = await _paintRepository.GetColorsAsync(cancellationToken);
        var finishes = await _paintRepository.GetFinishesAsync(cancellationToken);
        var surfaces = await _paintRepository.GetSurfacesAsync(cancellationToken);

        return new PaintLookupsResponse(brands, colors, finishes, surfaces);
    }

    public async Task<PaintResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paint = await GetRequiredAsync(id, cancellationToken);

        return paint.ToResponse();
    }

    public async Task<PaintResponse> CreateAsync(CreatePaintRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var paint = request.ToEntity();

        await _paintRepository.AddAsync(paint, cancellationToken);
        await _paintRepository.SaveChangesAsync(cancellationToken);

        return paint.ToResponse();
    }

    public async Task<PaintResponse> UpdateAsync(Guid id, UpdatePaintRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var transaction = await _transactionFactory.BeginAsync(cancellationToken);

        var paint = await LockRequiredAsync(id, cancellationToken);

        await EnsureStockCoversBasketsAsync(id, request.StockQuantity, cancellationToken);

        request.ApplyTo(paint);
        await _paintRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return paint.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _transactionFactory.BeginAsync(cancellationToken);

        var paint = await LockRequiredAsync(id, cancellationToken);

        await _basketRepository.RemovePaintFromAllAsync(id, cancellationToken);

        _paintRepository.Delete(paint);
        await _paintRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<Paint> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _paintRepository.GetByIdAsync(id, cancellationToken) ?? throw new PaintNotFoundException(id);
    }

    private async Task<Paint> LockRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _paintRepository.GetByIdForUpdateAsync(id, cancellationToken) ?? throw new PaintNotFoundException(id);
    }

    private async Task EnsureStockCoversBasketsAsync(Guid paintId, int stockQuantity, CancellationToken cancellationToken)
    {
        if (await _basketRepository.ExistsItemExceedingQuantityAsync(paintId, stockQuantity, cancellationToken))
        {
            throw new PaintStockBelowBasketQuantityException(paintId, stockQuantity);
        }
    }
}
