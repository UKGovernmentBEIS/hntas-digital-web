using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkEligibilityController : Controller
    {

        [HttpGet]
        public IActionResult RunningAHN()
        {
            Utility.ShowBackButton(this, "Guidance", "Guidance");
            return View(new RunningAHNViewModel());
        }

        [HttpPost]
        public IActionResult RunningAHN(RunningAHNViewModel model)
        {
            Utility.ShowBackButton(this, "Guidance", "Guidance");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsRunningHeatNetwork == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return View("ServesGt10Dwellings", new ServesGt10DwellingsViewModel());
        }

        [HttpGet]
        public IActionResult ServesGt10Dwellings()
        {
            Utility.ShowBackButton(this, "RunningAHN", "HeatNetworkEligibility");
            return View(new ServesGt10DwellingsViewModel());
        }

        [HttpPost]
        public IActionResult ServesGt10Dwellings(ServesGt10DwellingsViewModel model)
        {
            Utility.ShowBackButton(this, "RunningAHN", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.ServesMoreThan10Dwellings == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return View("LocatedInUk", new LocatedInUkViewModel());
        }

        [HttpGet]
        public IActionResult LocatedInUk()
        {
            Utility.ShowBackButton(this, "ServesGt10Dwellings", "HeatNetworkEligibility");
            return View(new LocatedInUkViewModel());
        }

        [HttpPost]
        public IActionResult LocatedInUk(LocatedInUkViewModel model)
        {
            Utility.ShowBackButton(this, "ServesGt10Dwellings", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsInUK == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }
            
            return View("OperatingAHN", new OperatingAHNViewModel());
        }

        [HttpGet]
        public IActionResult OperatingAHN()
        {
            Utility.ShowBackButton(this, "LocatedInUk", "HeatNetworkEligibility");
            return View(new OperatingAHNViewModel());
        }

        [HttpPost]
        public IActionResult OperatingAHN(OperatingAHNViewModel model)
        {
            Utility.ShowBackButton(this, "LocatedInUk", "HeatNetworkEligibility");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
