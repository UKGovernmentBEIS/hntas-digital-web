using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HNTAS.Web.UI.Filters
{
    public class EnsureSessionForOrganisationFlowOnGetAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();

            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(
                context.HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            if (controllerName == "Organisation" && organisationModel == null)
            {
                context.Result = new RedirectToActionResult("Start", "Organisation", null);
                return;
            }

            if (controllerName == "User")
            {
                var userModel = SessionHelper.GetFromSession<UserModel>(
                    context.HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

                if (organisationModel == null || userModel == null)
                {
                    context.Result = new RedirectToActionResult("Start", "Organisation", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}