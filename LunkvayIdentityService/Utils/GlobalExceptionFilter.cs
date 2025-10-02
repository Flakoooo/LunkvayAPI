using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace LunkvayIdentityService.Utils
{
    public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger = logger;

        public void OnException(ExceptionContext context)
        {
            Exception ex = context.Exception;
            _logger.LogError(ex, "Произошло исключение: {Message}", ex.Message);

            ProblemDetails problemDetails = new()
            {
                Title = "Ошибка",
                Detail = ex.Message
            };

            context.Result = ex switch
            {
                KeyNotFoundException => new NotFoundObjectResult(problemDetails), // 404
                UnauthorizedAccessException => new ObjectResult(problemDetails)
                {
                    StatusCode = (int?)HttpStatusCode.Forbidden // 403
                },
                _ => new ObjectResult(problemDetails)
                { 
                    StatusCode = (int?)HttpStatusCode.InternalServerError // 500
                }
            };

            context.ExceptionHandled = true;
        }
    }
}
