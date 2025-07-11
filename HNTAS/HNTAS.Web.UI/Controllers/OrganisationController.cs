using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace HNTAS.Web.UI.Controllers
{
    public class OrganisationController : Controller
    {
        // Key for storing the model in TempData
        private const string OrganisationModelSessionKey = "OrganisationModelKey";
        private readonly ICompaniesHouseService _companiesHouseService;

        public OrganisationController(ICompaniesHouseService companiesHouseService)
        {
            _companiesHouseService = companiesHouseService;
        }

        public IActionResult Type()
        {
            var model = GetOrganisationModelFromSession() ?? new OrganisationModel();
            // Always populate OrganisationTypes just before rendering the view
            model.OrganisationTypes = GetOrganisationTypeOptions();

            return View(model);
        }

        // POST: /Organisation/Type
        // This action handles the form submission.
        [HttpPost]
        [ValidateAntiForgeryToken] // Important for security to prevent Cross-Site Request Forgery
        public IActionResult Type(OrganisationModel model)
        {
            // Validate only the properties relevant to Type screen
            // Clear validation errors for other properties to avoid premature validation
            ModelState.Remove(nameof(model.CompanyNumber));
            ModelState.Remove(nameof(model.CompanyDetails)); // Also remove validation for CompanyDetails

            if (ModelState.IsValid)
            {
                // Retrieve the display text from the OrganisationTypes list
                // model.SelectedOrganisationType will contain the string like "UkCompaniesHouse"
                model.SelectedOrganisationTypeText = GetOrganisationTypeOptions()
                                                      .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?
                                                      .Text;
                // Save the current state of the model to TempData for the next step
                SaveOrganisationModelToSession(model);
                return RedirectToAction("CompanyNumber");
            }

            // If validation fails, repopulate OrganisationTypes before returning the view
            model.OrganisationTypes = GetOrganisationTypeOptions();

            return View("Type", model);
        }

        // GET: /Organisation/CompanyNumber
        public IActionResult CompanyNumber()
        {
            var model = GetOrganisationModelFromSession();
            if (model == null)
            {
                // If model is not found (e.g., user navigated directly), redirect to start
                return RedirectToAction("Type");
            }

            // Set ViewBag properties for the back button
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Type", "Organisation"); // Dynamically get URL for Screen 1

            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
            // Retrieve the full model from TempData to preserve previous steps' data
            var fullModel = GetOrganisationModelFromSession();
            if (fullModel == null)
            {
                return RedirectToAction("Type"); // Redirect if session expired or direct access
            }

            // Update only the relevant property from the submitted form (CompanyNumber)
            fullModel.CompanyNumber = model.CompanyNumber;

            // Clear ModelState and re-validate the entire model for final submission
            ModelState.Clear();
            TryValidateModel(fullModel);

            // Remove validation errors for properties not on this screen (SelectedOrganisationType)
            ModelState.Remove(nameof(fullModel.SelectedOrganisationType));

            CompanyDetailsModel? companyDetails = null; // Declare companyDetails here

            if (ModelState.IsValid)
            {
                try
                {
                    companyDetails = await _companiesHouseService.GetCompanyByNumberAsync(fullModel.CompanyNumber);
                    if (companyDetails == null)
                    {
                        // Company not found or API returned 404
                        ModelState.AddModelError(nameof(fullModel.CompanyNumber), "Company number not found. Please check and try again.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle network errors or other non-404 HTTP errors from API
                    ModelState.AddModelError(nameof(fullModel.CompanyNumber), "Could not verify company number at this time. Please try again later.");
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected errors during API call or parsing
                    ModelState.AddModelError(nameof(fullModel.CompanyNumber), "An unexpected error occurred during company number verification.");
                }
            }

            if (ModelState.IsValid && companyDetails != null) // Re-check ModelState after API validation and ensure companyDetails is not null
            {
                // Assign the fetched company details to the full model
                fullModel.CompanyDetails = companyDetails;

                // Save the updated full model (which now contains CompanyDetails) to session
                SaveOrganisationModelToSession(fullModel);

                return RedirectToAction("CompanyConfirm"); // Go to the confirmation screen
            }

            // If validation fails (either model validation or API validation), return to Screen 2 with errors
            // Re-set ViewBag properties if returning to the view with errors
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Type", "Organisation");

            return View("CompanyNumber", fullModel);
        }

        // GET: /Organisation/CompanyConfirm
        public IActionResult CompanyConfirm()
        {
            var organisationModel = GetOrganisationModelFromSession();

            if (organisationModel == null || organisationModel.CompanyDetails == null)
            {
                // If data is missing, redirect to the start or company number entry
                return RedirectToAction("CompanyNumber"); // Go back to company number entry if details are missing
            }

            // Set ViewBag for back link on confirmation page
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyNumber", "Organisation"); // Back to company number entry

            // Pass the CompanyDetails from the OrganisationTypeModel to the view
            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }


        // POST: /Organisation/ConfirmAndContinue 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAndContinue()
        {
            var organisationModel = GetOrganisationModelFromSession();

            if (organisationModel == null || organisationModel.CompanyDetails == null)
            {
                return RedirectToAction("Type"); // Redirect if session expired or data missing
            }



            // Clear all relevant session data as the process is complete
            //HttpContext.Session.Remove(OrganisationModelSessionKey);
            // Removed HttpContext.Session.Remove(CompanyDetailsModelSessionKey); as it's now part of OrganisationTypeModel

            //TempData["SubmissionMessage"] = "Organisation confirmed and data saved successfully!";
            // return RedirectToAction("Type"); // Redirect to start or a final success page

            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }


        // Helper to save model to Session
        private void SaveOrganisationModelToSession(OrganisationModel model)
        {
            string json = JsonConvert.SerializeObject(model);
            HttpContext.Session.SetString(OrganisationModelSessionKey, json);
        }

        // Helper to retrieve model from Session
        private OrganisationModel GetOrganisationModelFromSession()
        {
            string json = HttpContext.Session.GetString(OrganisationModelSessionKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<OrganisationModel>(json);
            }
            return null;
        }

        // Helper method to populate the OrganisationTypes list
        private List<SelectListItem> GetOrganisationTypeOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = OrganisationType.UkCompaniesHouse.ToString(), Text = "UK company registered with Companies House" }
            };
        }
    }
}
