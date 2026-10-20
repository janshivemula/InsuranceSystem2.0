using InsuranceSystem2._0.DTOs.Common;
using InsuranceSystem2._0.Exceptions;
using System.Net;
using System.Text.Json;

namespace InsuranceSystem2._0.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponseDto
            {
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path
            };

            switch (exception)
            {
                case BadRequestException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.ErrorType = "Bad Request";
                    response.Message = exception.Message;
                    break;

                case NotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.ErrorType = "Not Found";
                    response.Message = exception.Message;
                    break;

                case ConflictException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;

                    response.StatusCode = StatusCodes.Status409Conflict;
                    response.ErrorType = "Conflict";
                    response.Message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.ErrorType = "Forbidden";
                    response.Message = exception.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.ErrorType = "Internal Server Error";
                    response.Message = "An unexpected error occurred.";
                    break;
            }

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
