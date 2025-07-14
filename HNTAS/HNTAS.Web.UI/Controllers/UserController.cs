using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
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
            ModelState.Remove(nameof(model.ContactDetails) + "." + nameof(model.ContactDetails.EmailAddress));
            ModelState.Remove(nameof(model.ContactDetails) + "." + nameof(model.ContactDetails.FirstName));
            ModelState.Remove(nameof(model.ContactDetails) + "." + nameof(model.ContactDetails.LastName));
            ModelState.Remove(nameof(model.ContactDetails) + "." + nameof(model.ContactDetails.PreferredContactType));
            ModelState.Remove(nameof(model.ContactDetails) + "." + nameof(model.ContactDetails.JobTitle));

            if (!ModelState.IsValid)
            {
                var Orgmodel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
                model.OrganisationName = Orgmodel?.CompanyDetails?.Title ?? string.Empty;
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
                return RedirectToAction("ContactDetails", "User");
            }
            else
            {
                return RedirectToAction("Guidance", "Guidance");
            }
        }

        [HttpGet]
        public IActionResult ContactDetails()
        {
            // Always retrieve the comprehensive UserModel from session
            var fullModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);

            if (fullModel == null)
            {
                // If session model is missing, redirect back to the first step
                return RedirectToAction("ConfirmRPIsRC");
            }


            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ConfirmRPIsRC", "User");

            // Pass the ContactDetails sub-model to the view.
            // OrganisationName is static in the view, so no need to set it here from the model for display.
            return View(fullModel.ContactDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveContactDetails(ContactDetailsModel contactDetails)
        {
            var fullModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);

            if (fullModel == null)
            {
                return RedirectToAction("ConfirmRPIsRC"); // Redirect if session is lost
            }

            // Re-populate EmailAddress as it's a display-only field not directly posted
            if (string.IsNullOrEmpty(contactDetails.EmailAddress))
            {
                contactDetails.EmailAddress = fullModel.ContactDetails?.EmailAddress;
            }

            // --- Server-side conditional validation and clearing ---
            // Remove validation for fields that are not relevant based on selected type
            if (contactDetails.PreferredContactType == "Landline")
            {
                // Landline is selected, so MobileNumber is not required and should be cleared
                contactDetails.MobileNumber = null;
                ModelState.Remove(nameof(contactDetails.MobileNumber));

                // Add required validation for LandlineNumber if it's the selected type
                if (string.IsNullOrWhiteSpace(contactDetails.LandlineNumber))
                {
                    ModelState.AddModelError(nameof(contactDetails.LandlineNumber), "Enter your landline number");
                }
            }
            else if (contactDetails.PreferredContactType == "Mobile")
            {
                // Mobile is selected, so LandlineNumber and ContactNumberExtension are not required and should be cleared
                contactDetails.LandlineNumber = null;
                contactDetails.ContactNumberExtension = null;
                ModelState.Remove(nameof(contactDetails.LandlineNumber));
                ModelState.Remove(nameof(contactDetails.ContactNumberExtension));

                // Add required validation for MobileNumber if it's the selected type
                if (string.IsNullOrWhiteSpace(contactDetails.MobileNumber))
                {
                    ModelState.AddModelError(nameof(contactDetails.MobileNumber), "Enter your mobile number");
                }
            }
            else // No preferred contact type selected (e.g., on initial submission if none selected)
            {
                // Clear both if no type is chosen, or if it's somehow invalid
                contactDetails.LandlineNumber = null;
                contactDetails.ContactNumberExtension = null;
                contactDetails.MobileNumber = null;
                ModelState.Remove(nameof(contactDetails.LandlineNumber));
                ModelState.Remove(nameof(contactDetails.ContactNumberExtension));
                ModelState.Remove(nameof(contactDetails.MobileNumber));
            }

            // Now check overall model state
            if (!ModelState.IsValid)
            {
                return View("ContactDetails", contactDetails); // Return with errors
            }

            // If valid, save the updated contact details to the full model in session
            fullModel.ContactDetails = contactDetails;
            SessionHelper.SaveToSession(HttpContext, UserCreationSessionKey, fullModel);

            return RedirectToAction("CheckAnswers", "User");
        }

        [HttpGet]
        public IActionResult CheckAnswers() // Renamed from ReviewDetails
        {

            var userData = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey) ?? new UserModel();

            var organisationData = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);

            // Create a Tuple to pass both models to the view
            var viewModel = new Tuple<OrganisationModel, UserModel>(organisationData, userData);


            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("ContactDetails", "User");

            // Return the view, passing the populated view model
            return View("CheckAnswers", viewModel);
            
        }

        // You might also have a POST action to handle the "Accept and send" button submission
        [HttpPost]
        public IActionResult SubmitAnswers()
        {
            // Logic to process the submission, e.g., save data to a database
            // Redirect to a confirmation page or another relevant page
            return RedirectToAction("Confirmation","User"); // Example: Redirect to a Confirmation action
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmationAsync()
        {
            var userModel = SessionHelper.GetFromSession<UserModel>(HttpContext, UserCreationSessionKey);
            var emailAddress = userModel?.ContactDetails?.EmailAddress;

            if (string.IsNullOrEmpty(emailAddress))
            {
                // Handle the case where emailAddress is null or empty
                return RedirectToAction("ContactDetails", "User"); // Redirect to ContactDetails to ensure email is provided
            }
    
            ViewBag.CompanyName = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey)?.CompanyDetails?.Title;
            var company = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey)?.CompanyDetails;


         
            await _govUkNotifyService.SendEmailAsync(
                emailAddress,
                "297e670f-d6c8-49f2-b0d7-abe77256318a", // Template ID for confirmation email
                new Dictionary<string, dynamic>
                {
                    { "orgName", ViewBag.CompanyName },
                    { "orgId", "AC0000001" },
                    { "fullName", StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.FirstName) + " " + StringFormatter.ToTitleCaseSingleWord(userModel?.ContactDetails.LastName)},
                    { "address", StringFormatter.FormatAddress(company.RegisteredOfficeAddress) }
                }
            );

            return View("Confirmation");
        }
    }
}