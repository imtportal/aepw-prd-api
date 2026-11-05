using AEPW.Application.DTOs.Logging;

namespace AEPW.Application.Interfaces.Repositories.Logging
{
    public interface IExceptionLogRepository
    {
        Task LogAsync(
            ExceptionLogDto exceptionLog,
            CancellationToken cancellationToken = default);
    }
}
