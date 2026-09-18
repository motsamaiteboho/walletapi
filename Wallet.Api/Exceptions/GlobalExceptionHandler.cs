using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Wallet.Application.Exceptions;
using Wallet.Domain.Exceptions;

namespace Wallet.Api.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception processing request. Method={Method}, Path={Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var statusCode = exception switch
            {
                InsufficientFundsException =>
                    StatusCodes.Status422UnprocessableEntity,

                ArgumentException =>
                    StatusCodes.Status400BadRequest,

                WalletConcurrencyException =>
                    StatusCodes.Status409Conflict,

                _ =>
                    StatusCodes.Status500InternalServerError
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status400BadRequest =>
                        "Invalid request.",

                    StatusCodes.Status409Conflict =>
                        "Wallet was modified concurrently.",

                    StatusCodes.Status422UnprocessableEntity =>
                        "Withdrawal could not be completed.",

                    _ =>
                        "An unexpected error occurred."
                },
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }
    }
}
