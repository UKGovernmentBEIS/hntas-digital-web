using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkEligibilityController : Controller
    {
        private const string runningAHNModelKey = "runningAHN";
        private const string servesGt10DwellingsModelKey = "servesGt10Dwellings";
        private const string locatedInUkModelKey = "locatedInUk";
        private const string operatingAHNModelKey = "operatingAHN";



        [HttpGet]
        public IActionResult RunningAHN()
        {
            var runningAHNViewModel = SessionHelper.GetFromSession<RunningAHNViewModel>(HttpContext, runningAHNModelKey) ?? new RunningAHNViewModel();
            return View(runningAHNViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RunningAHN(RunningAHNViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SessionHelper.SaveToSession<RunningAHNViewModel>(HttpContext, runningAHNModelKey, model);

            if (model.IsRunningHeatNetwork == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return RedirectToAction("ServesGt10Dwellings");
        }

        [HttpGet]
        public IActionResult ServesGt10Dwellings()
        {
            this.ShowBackButton("RunningAHN", "HeatNetworkEligibility");
            var servesGt10DwellingsViewModel = SessionHelper.GetFromSession<ServesGt10DwellingsViewModel>(HttpContext, servesGt10DwellingsModelKey) ?? new ServesGt10DwellingsViewModel();
            return View(servesGt10DwellingsViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ServesGt10Dwellings(ServesGt10DwellingsViewModel model)
        {
            this.ShowBackButton("RunningAHN", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SessionHelper.SaveToSession<ServesGt10DwellingsViewModel>(HttpContext, servesGt10DwellingsModelKey, model);

            if (model.ServesMoreThan10Dwellings == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return RedirectToAction("LocatedInUk");
        }

        [HttpGet]
        public IActionResult LocatedInUk()
        {
            this.ShowBackButton("ServesGt10Dwellings", "HeatNetworkEligibility");
            var locatedInUkViewModel = SessionHelper.GetFromSession<LocatedInUkViewModel>(HttpContext, locatedInUkModelKey) ?? new LocatedInUkViewModel();
            return View(locatedInUkViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LocatedInUk(LocatedInUkViewModel model)
        {
            this.ShowBackButton("ServesGt10Dwellings", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SessionHelper.SaveToSession<LocatedInUkViewModel>(HttpContext, locatedInUkModelKey, model);

            if (model.IsInUK == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return RedirectToAction("OperatingAHN");
        }

        [HttpGet]
        public IActionResult OperatingAHN()
        {
            this.ShowBackButton("LocatedInUk", "HeatNetworkEligibility");
            var operatingAHNViewModel = SessionHelper.GetFromSession<OperatingAHNViewModel>(HttpContext, operatingAHNModelKey) ?? new OperatingAHNViewModel();
            return View(operatingAHNViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OperatingAHN(OperatingAHNViewModel model)
        {
            this.ShowBackButton("LocatedInUk", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SessionHelper.SaveToSession<OperatingAHNViewModel>(HttpContext, operatingAHNModelKey, model);

            if (model.IsExistingOrPlanned == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            // Eligible: show a message or redirect as needed
            ViewBag.ResultMessage = "You are eligible to register. Please create an account.";
            ViewBag.ShowCreateAccountButton = true; 
            return View(model);
        }
    }
}
