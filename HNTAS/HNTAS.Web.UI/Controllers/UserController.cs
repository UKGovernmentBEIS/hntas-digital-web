using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly GovUkNotifyService _govUkNotifyService;

        public UserController(GovUkNotifyService govUkNotifyService)
        {
            _govUkNotifyService = govUkNotifyService;
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ConfirmRPIsRC()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("CompanyConfirm", "Organisation");

            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult ConfirmRPIsRC(UserModel model)
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);

            foreach (var field in new[] { "EmailAddress", "FirstName", "LastName", "PreferredContactType", "JobTitle" })
            {
                ModelState.Remove($"{nameof(model.ContactDetails)}.{field}");
            }

            if (!ModelState.IsValid)
            {
                model.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = isCheckAnswerFlow
                    ? Url.Action("CheckAnswers", "User")
                    : Url.Action("CompanyConfirm", "Organisation");

                return View("ConfirmRPIsRC", model);
            }

            var sessionUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey) ?? new UserModel();

            if(sessionUserModel != null)
            {
                model.ContactDetails = sessionUserModel.ContactDetails;
            }

            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);

            if (model.ifRPisRC == true)
            {
                if (string.IsNullOrEmpty(model.ContactDetails.EmailAddress))
                {
                    model.ContactDetails.EmailAddress = User.FindFirstValue("email");
                    SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, model);
                }

                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                if (isCheckAnswerFlow)
                    return RedirectToAction("CheckAnswers", "User");

                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("Guidance", "Guidance");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult ContactDetails()
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = isCheckAnswerFlow
                ? Url.Action("CheckAnswers", "User")
                : Url.Action("ConfirmRPIsRC", "User");

            return View(userModel.ContactDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public IActionResult SaveContactDetails(ContactDetailsModel contactDetails)
        {
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
                contactDetails.EmailAddress = userModel.ContactDetails?.EmailAddress;

            switch (contactDetails.PreferredContactType)
            {
                case PreferredContactType.Landline:
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    if (string.IsNullOrWhiteSpace(contactDetails.LandlineNumber))
                        ModelState.AddModelError(nameof(contactDetails.LandlineNumber), "Enter your landline number.");
                    break;
                case PreferredContactType.Mobile:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    if (string.IsNullOrWhiteSpace(contactDetails.MobileNumber))
                        ModelState.AddModelError(nameof(contactDetails.MobileNumber), "Enter your mobile number.");
                    break;
                default:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    break;
            }

            if (!ModelState.IsValid)
            {
                bool isCheckAnswerFlow = SessionHelper.GetIsCheckAnswerFlow(HttpContext);
                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = isCheckAnswerFlow
                    ? Url.Action("CheckAnswers", "User")
                    : Url.Action("ConfirmRPIsRC", "User");

                return View("ContactDetails", contactDetails);
            }

            userModel.ContactDetails = contactDetails;
            SessionHelper.SaveToSession(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey, userModel);

            return RedirectToAction("CheckAnswers");
        }

        [HttpGet]
        [EnsureSessionForOrganisationFlowOnGet]
        public IActionResult CheckAnswers()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);           

            var viewModel = new CheckAnswersModel
            {
                Organisation = organisationModel,
                User = userModel,
                ConfirmedDeclaration = false
            };

            ViewBag.ShowBackButton = false;

            // Set the flow state to Check Answers mode
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, true);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnsureSessionForOrganisationFlowOnPost]
        public async Task<IActionResult> SubmitAnswers(CheckAnswersModel viewModel)
        {
            viewModel.Organisation = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, SessionHelper.SessionKeys.OrganisationCreation_SessionKey);
            viewModel.User = SessionHelper.GetFromSession<UserModel>(HttpContext, SessionHelper.SessionKeys.UserCreation_SessionKey);

            ModelState.Remove(nameof(viewModel.Organisation));
            ModelState.Remove(nameof(viewModel.User));

            if (!ModelState.IsValid)
            {
                ViewBag.ShowBackButton = false;
                return View("CheckAnswers", viewModel);
            }
            var organisationModel = viewModel.Organisation;
            var userModel = viewModel.User;

            var emailAddress = userModel?.ContactDetails?.EmailAddress;
            var company = organisationModel?.CompanyDetails;

            TempData["Confirmation_CompanyName"] = company?.Title;
            TempData["Confirmation_EmailAddress"] = emailAddress;

            await _govUkNotifyService.SendEmailAsync(
                emailAddress,
                "297e670f-d6c8-49f2-b0d7-abe77256318a",
                new Dictionary<string, dynamic>
                {
                    { "orgName", company?.Title },
                    { "orgId", "AC0000001" },
                    { "fullName", $"{StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.FirstName)} {StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.LastName)}" },
                    { "address", StringFormatter.FormatAddress(company?.RegisteredOfficeAddress) }
                }
            );

            SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
            SessionHelper.SetIsCheckAnswerFlow(HttpContext, false);

            return RedirectToAction("Confirmation", "User");
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation()
        {

            var companyName = TempData["Confirmation_CompanyName"] as string;
            var emailAddress = TempData["Confirmation_EmailAddress"] as string;


            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(emailAddress))
            {
                // Ensure any lingering session data is cleared before redirecting for a clean start.
                SessionHelper.ClearAllFlowRelatedSessionData(HttpContext);
                return RedirectToAction("Start", "Organisation"); // Redirect to the very beginning of the flow
            }

            ViewBag.CompanyName = companyName;
            ViewBag.EmailAddress = emailAddress;

            ViewBag.ShowBackButton = false;

            return View("Confirmation");
        }
    }
}