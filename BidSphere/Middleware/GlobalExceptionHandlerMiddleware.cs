using BidSphere.Exceptions;
using System.Net;
using System.Text.Json;

namespace BidSphere.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle 401 Unauthorized responses
                if (context.Response.StatusCode == 401 && !context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    var errorResponse = new
                    {
                        success = false,
                        message = "Authentication required. Please provide a valid token.",
                        statusCode = 401,
                        timestamp = DateTime.UtcNow
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }

                // Handle 403 Forbidden responses
                if (context.Response.StatusCode == 403 && !context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    var errorResponse = new
                    {
                        success = false,
                        message = "You do not have permission to access this resource.",
                        statusCode = 403,
                        timestamp = DateTime.UtcNow
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new
            {
                success = false,
                message = exception.Message,
                statusCode = 0,
                timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case AuctionNotFoundException:
                case ProductNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = errorResponse with { statusCode = response.StatusCode };
                    break;

                case InvalidBidException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = errorResponse with { statusCode = response.StatusCode };
                    break;

                case PaymentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = errorResponse with { statusCode = response.StatusCode };
                    break;

                case UnauthorizedBidException:
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse = errorResponse with { statusCode = response.StatusCode };
                    break;

                case ArgumentNullException:
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = errorResponse with { statusCode = response.StatusCode };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = errorResponse with
                    {
                        statusCode = response.StatusCode,
                        message = "An internal server error occurred. Please try again later."
                    };
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            return response.WriteAsync(result);
        }
    }
}
