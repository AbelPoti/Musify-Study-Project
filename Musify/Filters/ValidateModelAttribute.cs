using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Musify.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;
        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.Any(a => a.Value == null))
            {
                _logger.LogWarning($"Request body is null in request context> {context}");
                context.Result = new BadRequestObjectResult("Request body cannot be null.");
                return;
            }

            if (!context.ModelState.IsValid)
            {
                _logger.LogWarning($"Model state is invalid: {context.ModelState}");
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
