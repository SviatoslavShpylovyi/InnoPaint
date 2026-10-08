using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;

namespace Innowise.Application.Interfaces;

public interface IPaintService
{
    Task<PaintPageResponse> GetAllAsync(PaintFilterRequest filter, CancellationToken cancellationToken = default);
    Task<PaintLookupsResponse> GetLookupsAsync(CancellationToken cancellationToken = default);
    Task<PaintResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaintResponse> CreateAsync(CreatePaintRequest request, CancellationToken cancellationToken = default);
    Task<PaintResponse> UpdateAsync(Guid id, UpdatePaintRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
