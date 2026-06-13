using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace ERPWebApp.Middleware
{
    public class CwaFeatureGateAttribute(string feature) : ActionFilterAttribute
    {
        private readonly string _feature = feature;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

            if (!await featureManager.IsEnabledAsync(_feature))
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/_FeatureDisabled.cshtml"
                };
                return;
            }

            await next();
        }
    }
}
