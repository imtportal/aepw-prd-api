using AEPW.Application.DTOs.Program;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Interfaces.Repositories.Program

{
    public interface IProgramRepository
    {
        Task<PagedResponse<ProgramDto>> GetAllProgramsAsync(
            GetProgramsRequestDto request,
            CancellationToken cancellationToken = default);
        Task<ProgramDetailDto?> GetProgramByIdAsync(long programId, 
        CancellationToken cancellationToken = default);

        Task InsertProgramAsync(InsertProgramRequestDto request, 
        CancellationToken cancellationToken = default);

        Task UpdateProgramAsync(UpdateProgramRequestDto request, 
        CancellationToken cancellationToken = default);
    }
}
