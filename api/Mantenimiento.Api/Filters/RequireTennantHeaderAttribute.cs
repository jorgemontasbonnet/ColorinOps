using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Mantenimiento.Api.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireTennantHeaderAttribute : Attribute, IActionFilter
    {
        private const string TennantHeader = "X-CBS-TENNANT";

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var hasHeader = context.HttpContext.Request.Headers.TryGetValue(TennantHeader, out var value)
                            && !string.IsNullOrWhiteSpace(value);

            if (!hasHeader)
            {
                context.Result = new BadRequestObjectResult("Debe especificar una empresa válida");
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action required after execution
        }
    }
}