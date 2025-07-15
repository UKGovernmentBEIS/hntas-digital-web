using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HNTAS.Web.UI.Controllers
{
    public class UserController : Controller
    {
        private const string UserCreationSessionKey = "UserCreationModel";
        private const string OrganisationModelSessionKey = "OrganisationModelKey";
        private readonly GovUkNotifyService _govUkNotifyService;

        public UserController(GovUkNotifyService govUkNotifyService)
        {
            _govUkNotifyService = govUkNotifyService;
        }

        [HttpGet]
        public IActionResult ConfirmRPIsRC()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey) ?? new UserModel();
            var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            userModel.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyConfirm", "Organisation");

            return View(userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmRPIsRC(UserModel model)
        {
            // Remove validation for ContactDetails fields
            foreach (var field in new[] { "EmailAddress", "FirstName", "LastName", "PreferredContactType", "JobTitle" })
            {
                ModelState.Remove($"{nameof(model.ContactDetails)}.{field}");
            }

            if (!ModelState.IsValid)
            {
                var orgModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
                model.OrganisationName = orgModel?.CompanyDetails?.Title ?? string.Empty;
                return View("ConfirmRPIsRC", model);
            }

            SessionHelper.SaveToSession(HttpContext, UserCreationSessionKey, model);

            if (model.ifRPisRC == true)
            {
                if (string.IsNullOrEmpty(model.ContactDetails.EmailAddress))
                {
                    model.ContactDetails.EmailAddress = User.FindFirstValue("email");
                    SessionHelper.SaveToSession(HttpContext, UserCreationSessionKey, model);
                }
                return RedirectToAction("ContactDetails");
            }
            return RedirectToAction("Guidance", "Guidance");
        }

        [HttpGet]
        public IActionResult ContactDetails()
        {
            var fullModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);
            if (fullModel == null)
                return RedirectToAction("ConfirmRPIsRC");

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRPIsRC", "User");
            return View(fullModel.ContactDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveContactDetails(ContactDetailsModel contactDetails)
        {
            var fullModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);
            if (fullModel == null)
                return RedirectToAction("ConfirmRPIsRC");

            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
                contactDetails.EmailAddress = fullModel.ContactDetails?.EmailAddress;

            // Conditional validation
            switch (contactDetails.PreferredContactType)
            {
                case PreferredContactType.Landline:
                    contactDetails.MobileNumber = null;
                    ModelState.Remove(nameof(contactDetails.MobileNumber));
                    if (string.IsNullOrWhiteSpace(contactDetails.LandlineNumber))
                        ModelState.AddModelError(nameof(contactDetails.LandlineNumber), "Enter your landline number");
                    break;
                case PreferredContactType.Mobile:
                    contactDetails.LandlineNumber = null;
                    contactDetails.ContactNumberExtension = null;
                    ModelState.Remove(nameof(contactDetails.LandlineNumber));
                    ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                    if (string.IsNullOrWhiteSpace(contactDetails.MobileNumber))
                        ModelState.AddModelError(nameof(contactDetails.MobileNumber), "Enter your mobile number");
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
                return View("ContactDetails", contactDetails);

            fullModel.ContactDetails = contactDetails;
            SessionHelper.SaveToSession(HttpContext, UserCreationSessionKey, fullModel);

            return RedirectToAction("CheckAnswers");
        }

        [HttpGet]
        public IActionResult CheckAnswers()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);

            if (organisationModel == null || userModel == null)
                return RedirectToAction("ConfirmRPIsRC");

            var viewModel = new CheckAnswersModel
            {
                Organisation = organisationModel,
                User = userModel,
                ConfirmedDeclaration = false
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SubmitAnswers(CheckAnswersModel viewModel)
        {
            viewModel.Organisation = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            viewModel.User = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);

            if (viewModel.Organisation == null || viewModel.User == null)
                return RedirectToAction("ConfirmRPIsRC");

            ModelState.Remove(nameof(viewModel.Organisation));
            ModelState.Remove(nameof(viewModel.User));

            if (!ModelState.IsValid)
                return View("CheckAnswers", viewModel);

            var fullUserModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);
            if (fullUserModel == null)
                return RedirectToAction("ConfirmRPIsRC");

            return RedirectToAction("Confirmation", "User");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmationAsync()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);
            var emailAddress = userModel?.ContactDetails?.EmailAddress;

            if (string.IsNullOrEmpty(emailAddress))
                return RedirectToAction("ContactDetails");

            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            var company = organisationModel?.CompanyDetails;
            ViewBag.CompanyName = company?.Title;

            await _govUkNotifyService.SendEmailAsync(
                emailAddress,
                "297e670f-d6c8-49f2-b0d7-abe77256318a",
                new Dictionary<string, dynamic>
                {
                    { "orgName", ViewBag.CompanyName },
                    { "orgId", "AC0000001" },
                    { "fullName", $"{StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.FirstName)} {StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.LastName)}" },
                    { "address", StringFormatter.FormatAddress(company?.RegisteredOfficeAddress) }
                }
            );

            return View("Confirmation");
        }
    }
}