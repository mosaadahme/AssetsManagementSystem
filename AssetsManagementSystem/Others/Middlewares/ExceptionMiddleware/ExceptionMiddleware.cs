namespace AssetsManagementSystem.Others.Middlewares.ExceptionMiddleware
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                await next(httpContext);



            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            httpContext.Response.ContentType = "application/json";

            int statusCode = GetStatusCode(exception);
            httpContext.Response.StatusCode = statusCode;

            var errors = new List<string>
            {
                $"Message: {exception.Message}",
                $"Source: {exception.Source}",
                exception.InnerException != null ? $"Inner Exception: {exception.InnerException.Message}" : null
            }.Where(e => e != null).ToList();

            var response = new ExceptionModel
            {
                StatusCode = statusCode,
                errors = errors
            };

             _logger.LogError(exception, "An error occurred at {Time}: {Message}", DateTime.UtcNow, exception.Message);

             await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }

        private static int GetStatusCode(Exception exception) =>
            exception switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status422UnprocessableEntity,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                InvalidOperationException => StatusCodes.Status409Conflict,
                NullReferenceException => StatusCodes.Status400BadRequest,
                TimeoutException => StatusCodes.Status408RequestTimeout,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                AccessViolationException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
    }
}
