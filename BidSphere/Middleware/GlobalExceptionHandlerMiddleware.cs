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

            int statusCode;
            string message = exception.Message;

            // Check exception type and set appropriate status code
            if (exception is AuctionNotFoundException || exception is ProductNotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
            }
            else if (exception is PaymentException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
            }
            else if (exception is InvalidBidException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
            }
            else if (exception is UnauthorizedBidException || exception is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else if (exception is ArgumentNullException || exception is ArgumentException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An internal server error occurred. Please try again later.";
            }

            response.StatusCode = statusCode;

            var errorResponse = new
            {
                success = false,
                message = message,
                statusCode = statusCode,
                timestamp = DateTime.UtcNow
            };

            var result = JsonSerializer.Serialize(errorResponse);
            return response.WriteAsync(result);
        }
    }
}
