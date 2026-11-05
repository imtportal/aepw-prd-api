using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Logging;
using AEPW.Application.Interfaces.Repositories.Logging;
using AEPW.Application.Interfaces.Services.Logging;
namespace AEPW.Application.Services.Logging
{
    public sealed class ExceptionLogService : IExceptionLogService
    {
        private readonly IExceptionLogRepository _repository;

        public ExceptionLogService(
            IExceptionLogRepository repository)
        {
            _repository = repository;
        }

        public async Task LogAsync(
            ExceptionLogDto exceptionLog,
            CancellationToken cancellationToken = default)
        {
            await _repository.LogAsync(
                exceptionLog,
                cancellationToken);
        }
    }
}
