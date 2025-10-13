using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LunkvayAPI.Common.Filters
{
    public class AuthorizationFilter(ILogger<AuthorizationFilter> logger) : IActionFilter
    {
        private readonly ILogger<AuthorizationFilter> _logger = logger;

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
                return;

            string? idClaim = context.HttpContext.User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out Guid userId))
            {
                _logger.LogWarning("В токене отсутствует валидный идентификатор пользователя (claim 'id')");

                context.Result = new UnauthorizedObjectResult("Не удалось идентифицировать пользователя");

                return;
            }

            context.HttpContext.Items["UserId"] = userId;
        }
        public void OnActionExecuting(ActionExecutingContext context) { }
    }
}
