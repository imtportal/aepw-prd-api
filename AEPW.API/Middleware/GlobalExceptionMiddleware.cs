using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Logging;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Services;
using AEPW.Application.Interfaces.Services.Logging;
using AEPW.Domain.Constants;
using AEPW.API.Extensions;
using Microsoft.Data.SqlClient;
using ValidationException = AEPW.Application.Exceptions.ValidationException;

namespace AEPW.API.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions, logs them to the database,
/// writes to ILogger and returns a standard API response.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, IExceptionLogService exceptionLogService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, exceptionLogService);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception, IExceptionLogService exceptionLogService)
    {
        var (statusCode, errorCode, message, errors) = exception switch
        {
            NotFoundException e =>
                (HttpStatusCode.NotFound,
                 ErrorCodes.NotFound,
                 e.Message,
                 (IReadOnlyList<string>)[]),

            ValidationException e =>
                (HttpStatusCode.BadRequest,
                 ErrorCodes.ValidationFailed,
                 e.Message,
                 e.Errors),

            ConflictException e =>
                (HttpStatusCode.Conflict,
                 ErrorCodes.Conflict,
                 e.Message,
                 (IReadOnlyList<string>)[]),

            // Layer 2 (permanent vs transient processing failures) - see the contract comment on
            // EvidenceProcessingController.ProcessEvidence for what the Worker should do with these.
            PermanentProcessingException e =>
                (HttpStatusCode.UnprocessableEntity,
                 ErrorCodes.PermanentProcessingFailure,
                 e.Message,
                 (IReadOnlyList<string>)[]),

            TransientProcessingException e =>
                (HttpStatusCode.ServiceUnavailable,
                 ErrorCodes.TransientProcessingFailure,
                 e.Message,
                 (IReadOnlyList<string>)[]),

            SqlException e =>
            (
                HttpStatusCode.InternalServerError,
                ErrorCodes.DatabaseError,
                e.Message,
                (IReadOnlyList<string>)[]
            ),

            UnauthorizedAccessException e =>
                (HttpStatusCode.Unauthorized,
                 ErrorCodes.Unauthorized,
                 e.Message,
                 (IReadOnlyList<string>)[]),

            _ =>
                (HttpStatusCode.InternalServerError,
                 ErrorCodes.InternalError,
                 "An unexpected error occurred. Please try again later.",
                 (IReadOnlyList<string>)[])
        };

        var trace = new StackTrace(exception, true);

        var frame = trace.GetFrames()?
            .FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.GetFileName()));
        var userId = context.GetCurrentUserId();

        Guid? correlationId = null;

        if (Guid.TryParse(
                context.Items[CorrelationIdHeader]?.ToString(),
                out var parsedCorrelationId))
        {
            correlationId = parsedCorrelationId;
        }

        var dto = new ExceptionLogDto
        {
            Message = exception.Message,

            InnerException = exception.InnerException?.Message,

            StackTrace = exception.StackTrace,

            Source = exception.Source,

            ClassName = frame?.GetMethod()?.DeclaringType?.FullName,

            MethodName = frame?.GetMethod()?.Name,

            FileName = frame?.GetFileName(),

            LineNumber = frame?.GetFileLineNumber(),

            RequestPath = context.Request.Path,

            HttpMethod = context.Request.Method,

            QueryString = context.Request.QueryString.Value,

            CorrelationId = context.Items[CorrelationIdHeader]?.ToString(),

            UserId = userId,

            MachineName = Environment.MachineName,

            EnvironmentName = _environment.EnvironmentName
        };

        // Never let logging failures affect the API response.
        try
        {
            await exceptionLogService.LogAsync(dto);
        }
        catch (Exception logException)
        {
            _logger.LogError(
                logException,
                "Failed to persist exception to ExceptionLog table.");
        }

        _logger.LogError(
            exception,
            "Unhandled exception: {ErrorCode} - {Message} - Path: {Path}",
            errorCode,
            exception.Message,
            context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(
            message,
            errorCode,
            errors);

        var json = JsonSerializer.Serialize(
            response,
            _jsonOptions);

        await context.Response.WriteAsync(json);
    }
}