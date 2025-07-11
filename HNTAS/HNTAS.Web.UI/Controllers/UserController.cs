using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class UserController : Controller
    {
        private const string UserModelSessionKey = "UserModelKey"; // Define a constant for the session key
        private const string OrganisationModelSessionKey = "OrganisationModelKey";

        [HttpGet]
        [Route("User/Is-user-Regulatory-Contact")]
        public IActionResult ConfirmRPIsRC()
        {
            // Try to retrieve the model from session. If null, create a new one.
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserModelSessionKey) ?? new UserModel();
            var Orgmodel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);

            userModel.OrganisationName = Orgmodel?.CompanyDetails?.Title ?? string.Empty;

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm", "Organisation");

            return View(userModel);
        }

        [HttpPost]
        [Route("User/Is-user-Regulatory-Contact")]
        public IActionResult ConfirmRPIsRC(UserModel model)
        {

            if (!ModelState.IsValid)
            {
                var Orgmodel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
                model.OrganisationName = Orgmodel?.CompanyDetails?.Title ?? string.Empty;
                return View("ConfirmRPIsRC", model);
            }

            // Validation passed. Now, save the model to session for the next step.
            SessionHelper.SaveToSession(HttpContext, UserModelSessionKey, model);

            if (model.ifRPisRC == true) // Use '==' for nullable bool comparison
            {
                // Redirect to the next specific action for 'Yes'
                return RedirectToAction("ContactDetails", "User"); // Assuming ContactDetails is in UserController
            }
            return RedirectToAction("Guidance", "Guidance");
        }

        [HttpGet]
        public IActionResult ContactDetails()
        {
            return View("ContactDetails", new UserModel());
        }
    }
}
