using AEPW.Application.DTOs;
using AEPW.Domain.Models.BusinessRoleMapping;
using AEPW.Domain.Models.Lookup;
namespace AEPW.Application.Interfaces.Repositories.BusinessRoleMapping;

public interface IBusinessRoleMappingRepository
{
    Task<IReadOnlyList<BusinessRoleMappingLookupModel>> GetAllBusinessRoleMappingsAsync(long? userId,
    CancellationToken cancellationToken = default);
    Task<string> SaveBusinessRoleMappingAsync(
        SaveBusinessRoleMappingModel model,
        CancellationToken cancellationToken = default);

    Task<BusinessRoleMappingDetailModel?> GetBusinessRoleMappingByIdAsync(
        string businessRoleMappingId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteBusinessRoleMappingAsync(
        DeleteBusinessRoleMappingModel model,
        CancellationToken cancellationToken = default);

}