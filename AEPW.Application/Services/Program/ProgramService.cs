using AEPW.Application.DTOs.Program;
using AEPW.Application.Interfaces.Repositories.Program;
using AEPW.Application.Interfaces.Services.Program;
using AEPW.Application.Contracts.Responses;

namespace AEPW.Application.Services.Program;

public class ProgramService : IProgramService
{
    private readonly IProgramRepository _programRepository;

    public ProgramService(
        IProgramRepository programRepository)
    {
        _programRepository = programRepository;
    }

    public async Task<PagedResponse<ProgramDto>> GetAllProgramsAsync(
        GetProgramsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _programRepository.GetAllProgramsAsync(
            request,
            cancellationToken);
    }

    public async Task<ProgramDetailDto?> GetProgramByIdAsync(
        long programId,
        CancellationToken cancellationToken = default)
    {
        return await _programRepository.GetProgramByIdAsync(
            programId,
            cancellationToken);
    }

    public async Task InsertProgramAsync(
        InsertProgramRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _programRepository.InsertProgramAsync(
            request,
            cancellationToken);
    }

    public async Task UpdateProgramAsync(
        UpdateProgramRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _programRepository.UpdateProgramAsync(
            request,
            cancellationToken);
    }
}