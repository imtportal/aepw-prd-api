using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Logging;
namespace AEPW.Application.Interfaces.Services.Logging
{
    public interface IExceptionLogService
    {
        Task LogAsync(
            ExceptionLogDto exceptionLog,
            CancellationToken cancellationToken = default);
    }
}
