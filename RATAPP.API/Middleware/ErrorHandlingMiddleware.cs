using System.Net;
using System.Text.Json;
using RATAPP.API.Models;

namespace RATAPP.API.Middleware
{
    /// <summary>
    /// Global error handling middleware that catches all unhandled exceptions
    /// and returns them as properly formatted API responses.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, response) = exception switch
            {
                KeyNotFoundException e => (
                    HttpStatusCode.NotFound,
                    ApiResponse<object>.Error(e.Message)
                ),
                InvalidOperationException e => (
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Error(e.Message)
                ),
                ArgumentException e => (
                    HttpStatusCode.BadRequest,
                    ApiResponse<object>.Error(e.Message)
                ),
                UnauthorizedAccessException e => (
                    HttpStatusCode.Unauthorized,
                    ApiResponse<object>.Error("Unauthorized access")
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    ApiResponse<object>.Error(
                        "An unexpected error occurred",
                        "Please contact support if the problem persists"
                    )
                )
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// Extension method to easily add the error handling middleware to the application pipeline
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
