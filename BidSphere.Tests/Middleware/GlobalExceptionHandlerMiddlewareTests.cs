using BidSphere.Exceptions;
using BidSphere.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Xunit;

namespace BidSphere.Tests.Middleware
{
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock;

        public GlobalExceptionHandlerMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        }

        [Fact]
        public async Task InvokeAsync_WithNoException_ShouldCallNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var nextCalled = false;
            RequestDelegate next = (HttpContext ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            nextCalled.Should().BeTrue();
        }

        [Fact]
        public async Task InvokeAsync_WithProductNotFoundException_ShouldReturn404()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new ProductNotFoundException(1);
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("Product with ID 1 not found");
        }

        [Fact]
        public async Task InvokeAsync_WithAuctionNotFoundException_ShouldReturn404()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new AuctionNotFoundException(5);
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            context.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidBidException_ShouldReturn400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new InvalidBidException("Bid amount too low");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("Bid amount too low");
        }

        [Fact]
        public async Task InvokeAsync_WithPaymentException_ShouldReturn400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new PaymentException("Payment failed");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            context.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthorizedBidException_ShouldReturn403()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new UnauthorizedBidException("Cannot bid on own product");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("Cannot bid on own product");
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturn403()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new UnauthorizedAccessException("Access denied");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
            context.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithArgumentNullException_ShouldReturn400()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new ArgumentNullException("parameter");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            context.Response.ContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task InvokeAsync_WithGenericException_ShouldReturn500()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                throw new Exception("Unexpected error");
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("An internal server error occurred");
        }

        [Fact]
        public async Task InvokeAsync_With401Response_ShouldAddJsonMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("Authentication required");
        }

        [Fact]
        public async Task InvokeAsync_With403Response_ShouldAddJsonMessage()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            RequestDelegate next = (HttpContext ctx) =>
            {
                ctx.Response.StatusCode = 403;
                return Task.CompletedTask;
            };

            var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.Should().Be(403);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Contain("You do not have permission");
        }
    }
}
