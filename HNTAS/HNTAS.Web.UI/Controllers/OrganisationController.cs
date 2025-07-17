using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Drawing2D;

namespace HNTAS.Web.UI.Controllers
{
    public class OrganisationController : Controller
    {
        private const string OrganisationModelSessionKey = "OrganisationModelKey";
        private readonly ICompaniesHouseService _companiesHouseService;

        public OrganisationController(ICompaniesHouseService companiesHouseService)
        {
            _companiesHouseService = companiesHouseService;
        }

        public IActionResult Type()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey) ?? new OrganisationModel();
            model.OrganisationTypes = GetOrganisationTypeOptions();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Type(OrganisationModel model)
        {
            ModelState.Remove(nameof(model.CompanyNumber));
            ModelState.Remove(nameof(model.CompanyDetails));

            if (ModelState.IsValid)
            {
                model.SelectedOrganisationTypeText = GetOrganisationTypeOptions()
                    .FirstOrDefault(item => item.Value == model.SelectedOrganisationType)?.Text;
                if(model.SelectedOrganisationType == OrganisationType.Other.ToString())
                {
                    ModelState.AddModelError(nameof(model.SelectedOrganisationType), "Not applicable for this scope");
                    model.OrganisationTypes = GetOrganisationTypeOptions();
                    return View("Type", model);
                }
                SessionHelper.SaveToSession(HttpContext, OrganisationModelSessionKey, model);
                return RedirectToAction("CompanyNumber");
            }

            model.OrganisationTypes = GetOrganisationTypeOptions();
            return View("Type", model);
        }

        public IActionResult CompanyNumber()
        {
            var model = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            if (model == null)
            {
                return RedirectToAction("Type");
            }

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Type", "Organisation");
            return View("CompanyNumber", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompanyNumberAsync(OrganisationModel model)
        {
            var fullModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);
            if (fullModel == null)
            {
                return RedirectToAction("Type");
            }

            fullModel.CompanyNumber = model.CompanyNumber;
            ModelState.Clear();
            TryValidateModel(fullModel);
            ModelState.Remove(nameof(fullModel.SelectedOrganisationType));

            CompanyDetailsModel? companyDetails = null;

            if (ModelState.IsValid)
            {
                try
                {
                    companyDetails = await _companiesHouseService.GetCompanyByNumberAsync(fullModel.CompanyNumber);
                    if (companyDetails == null)
                    {
                        ModelState.AddModelError(nameof(fullModel.CompanyNumber), "Company number not found. Please check and try again.");
                    }
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(nameof(fullModel.CompanyNumber), "Could not verify company number at this time. Please try again later.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(nameof(fullModel.CompanyNumber), "An unexpected error occurred during company number verification.");
                }
            }

            if (ModelState.IsValid && companyDetails != null)
            {
                fullModel.CompanyDetails = companyDetails;
                SessionHelper.SaveToSession(HttpContext, OrganisationModelSessionKey, fullModel);
                return RedirectToAction("CompanyConfirm");
            }

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Type", "Organisation");
            return View("CompanyNumber", fullModel);
        }

        public IActionResult CompanyConfirm()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);

            if (organisationModel == null || organisationModel.CompanyDetails == null)
            {
                return RedirectToAction("CompanyNumber");
            }

            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("CompanyNumber", "Organisation");
            return View("CompanyConfirm", organisationModel.CompanyDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAndContinue()
        {
            var organisationModel = SessionHelper.GetFromSession<OrganisationModel>(HttpContext, OrganisationModelSessionKey);

            if (organisationModel == null || organisationModel.CompanyDetails == null)
            {
                return RedirectToAction("CompanyNumber");
            }

            return RedirectToAction("ConfirmRPIsRC", "User");
        }

        private List<SelectListItem> GetOrganisationTypeOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = OrganisationType.UkCompaniesHouse.ToString(), Text = "UK company registered with Companies House" },
                new SelectListItem { Value = OrganisationType.Other.ToString(), Text = "Other" }
            };
        }
    }
}
