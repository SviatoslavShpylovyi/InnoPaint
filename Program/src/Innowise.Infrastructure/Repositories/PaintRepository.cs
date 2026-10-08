using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Innowise.Application.DTOs.Requests;
using Innowise.Application.Interfaces;
using Innowise.Domain.Models;
using Innowise.Infrastructure.Data;

namespace Innowise.Infrastructure.Repositories;

public sealed class PaintRepository : IPaintRepository
{
    private readonly InnoPaintDbContext _context;

    public PaintRepository(InnoPaintDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Paint>> GetAllAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = Sort(Filter(_context.Paints.AsNoTracking(), filter), filter);
        var offset = ((long)filter.Page - 1) * filter.PageSize;

        if (offset > int.MaxValue)
        {
            return [];
        }

        return await query.Skip((int)offset).Take(filter.PageSize).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        return await Filter(_context.Paints.AsNoTracking(), filter).CountAsync(cancellationToken);
    }

    public async Task<Paint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Paints.FirstOrDefaultAsync(paint => paint.Id == id, cancellationToken);
    }

    public async Task<Paint?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureTransactionIsActive();

        var paints = await _context.Paints
            .FromSql($"SELECT * FROM `Paints` WHERE `Id` = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);

        return paints.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Paint>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.Paints
            .AsNoTracking()
            .Where(paint => ids.Contains(paint.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return await GetDistinctValuesAsync(paint => paint.Brand, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetColorsAsync(CancellationToken cancellationToken = default)
    {
        return await GetDistinctValuesAsync(paint => paint.Color, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetFinishesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDistinctValuesAsync(paint => paint.Finish, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetSurfacesAsync(CancellationToken cancellationToken = default)
    {
        return await GetDistinctValuesAsync(paint => paint.Surface, cancellationToken);
    }

    public async Task AddAsync(Paint paint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paint);

        await _context.Paints.AddAsync(paint, cancellationToken);
    }

    public void Delete(Paint paint)
    {
        ArgumentNullException.ThrowIfNull(paint);

        _context.Paints.Remove(paint);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTransactionIsActive()
    {
        if (_context.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "A row lock only holds inside a transaction. Begin one with IStoreTransactionFactory before calling GetByIdForUpdateAsync.");
        }
    }

    private async Task<IReadOnlyList<string>> GetDistinctValuesAsync(
        Expression<Func<Paint, string>> selector,
        CancellationToken cancellationToken)
    {
        return await _context.Paints
            .AsNoTracking()
            .Select(selector)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Paint> Filter(IQueryable<Paint> query, PaintFilterRequest filter)
    {
        foreach (var term in SearchTerms(filter.Search))
        {
            query = query.Where(paint =>
                (paint.Name + " " + paint.Brand + " " + paint.Description + " " + paint.Color + " " + paint.Finish + " " + paint.Surface)
                    .Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
        {
            var brand = filter.Brand.Trim();
            query = query.Where(paint => paint.Brand == brand);
        }

        if (!string.IsNullOrWhiteSpace(filter.Color))
        {
            var color = filter.Color.Trim();
            query = query.Where(paint => paint.Color == color);
        }

        if (!string.IsNullOrWhiteSpace(filter.Finish))
        {
            var finish = filter.Finish.Trim();
            query = query.Where(paint => paint.Finish == finish);
        }

        if (!string.IsNullOrWhiteSpace(filter.Surface))
        {
            var surface = filter.Surface.Trim();
            query = query.Where(paint => paint.Surface == surface);
        }

        if (filter.IsExterior.HasValue)
        {
            query = query.Where(paint => paint.IsExterior == filter.IsExterior.Value);
        }

        if (filter.InStock.HasValue)
        {
            query = filter.InStock.Value
                ? query.Where(paint => paint.StockQuantity > 0)
                : query.Where(paint => paint.StockQuantity == 0);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(paint => paint.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(paint => paint.Price <= filter.MaxPrice.Value);
        }

        if (filter.MinVolumeLiters.HasValue)
        {
            query = query.Where(paint => paint.VolumeLiters >= filter.MinVolumeLiters.Value);
        }

        if (filter.MaxVolumeLiters.HasValue)
        {
            query = query.Where(paint => paint.VolumeLiters <= filter.MaxVolumeLiters.Value);
        }

        return query;
    }

    private static IQueryable<Paint> Sort(IQueryable<Paint> query, PaintFilterRequest filter)
    {
        var sorted = filter.SortBy.ToLowerInvariant() switch
        {
            "price" => filter.Descending ? query.OrderByDescending(paint => paint.Price) : query.OrderBy(paint => paint.Price),
            "volume" => filter.Descending ? query.OrderByDescending(paint => paint.VolumeLiters) : query.OrderBy(paint => paint.VolumeLiters),
            "brand" => filter.Descending ? query.OrderByDescending(paint => paint.Brand) : query.OrderBy(paint => paint.Brand),
            _ => filter.Descending ? query.OrderByDescending(paint => paint.Name) : query.OrderBy(paint => paint.Name)
        };

        return sorted.ThenBy(paint => paint.Id);
    }

    private static IReadOnlyList<string> SearchTerms(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        return search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
