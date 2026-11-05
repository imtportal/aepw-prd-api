using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Logging
{
    public sealed class ExceptionLogDto
    {
        public string Message { get; set; } = string.Empty;

        public string? InnerException { get; set; }

        public string? StackTrace { get; set; }

        public string? Source { get; set; }

        public string? ClassName { get; set; }

        public string? MethodName { get; set; }

        public string? FileName { get; set; }

        public int? LineNumber { get; set; }

        public string? RequestPath { get; set; }

        public string? HttpMethod { get; set; }

        public string? QueryString { get; set; }

        public string? CorrelationId { get; set; }

        public long? UserId { get; set; }

        public string? MachineName { get; set; }

        public string? EnvironmentName { get; set; }
    }
}
