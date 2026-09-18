namespace Wallet.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-ID";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(
            RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ILogger<CorrelationIdMiddleware> logger)
        {
            var correlationId =
                context.Request.Headers[HeaderName].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            context.Response.Headers[HeaderName] = correlationId;

            using (logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
            {
                logger.LogInformation(
                    "HTTP request started. Method={Method}, Path={Path}",
                    context.Request.Method,
                    context.Request.Path);

                await _next(context);

                logger.LogInformation(
                    "HTTP request completed. StatusCode={StatusCode}",
                    context.Response.StatusCode);
            }
        }
    }
}
