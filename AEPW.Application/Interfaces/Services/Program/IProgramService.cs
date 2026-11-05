using AEPW.Application.DTOs.Program;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Interfaces.Services.Program
{
public interface IProgramService
    {

        Task<PagedResponse<ProgramDto>> GetAllProgramsAsync(
            GetProgramsRequestDto request,
            CancellationToken cancellationToken = default);
        Task<ProgramDetailDto?> GetProgramByIdAsync(
            long programId,
            CancellationToken cancellationToken = default);

        Task InsertProgramAsync(
            InsertProgramRequestDto request,
            CancellationToken cancellationToken = default);

        Task UpdateProgramAsync(
            UpdateProgramRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
