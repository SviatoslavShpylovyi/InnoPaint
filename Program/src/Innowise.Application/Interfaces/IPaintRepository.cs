using Innowise.Application.DTOs.Requests;
using Innowise.Domain.Models;

namespace Innowise.Application.Interfaces;

public interface IPaintRepository
{
    Task<IReadOnlyList<Paint>> GetAllAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default);
    Task<int> CountAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Paint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Paint?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Paint>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetBrandsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetColorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetFinishesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetSurfacesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Paint paint, CancellationToken cancellationToken = default);
    void Delete(Paint paint);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
