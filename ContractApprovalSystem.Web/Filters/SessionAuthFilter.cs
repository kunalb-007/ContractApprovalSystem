using ContractApprovalSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ContractApprovalSystem.Web.Filters
{
    public class SessionAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!SessionHelper.IsAuthenticated(context.HttpContext))
            {
                context.Result = new RedirectToActionResult("Login", "Account", 
                    new { returnUrl = context.HttpContext.Request.Path });
            }
            
            base.OnActionExecuting(context);
        }
    }
    
    public class ManagerAuthFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!SessionHelper.IsAuthenticated(context.HttpContext))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
            else if (!SessionHelper.IsManager(context.HttpContext))
            {
                context.Result = new ForbidResult();
            }
            
            base.OnActionExecuting(context);
        }
    }
}
