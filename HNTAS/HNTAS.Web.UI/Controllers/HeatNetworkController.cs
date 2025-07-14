using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        public void showBackButton(string action, string controller) {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action(action, controller);
        }

        [HttpGet]
        public IActionResult RunningAHN()
        {
            return View(new RunningAHNViewModel());
        }

        [HttpPost]
        public IActionResult RunningAHN(RunningAHNViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
            showBackButton("RunningAHN", "HeatNetwork");
            return View(new ServesGt10DwellingsViewModel());
        }

        [HttpPost]
        public IActionResult ServesGt10Dwellings(ServesGt10DwellingsViewModel model)
        {
            showBackButton("RunningAHN", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
            showBackButton("ServesGt10Dwellings", "HeatNetwork");
            return View(new LocatedInUkViewModel());
        }

        [HttpPost]
        public IActionResult LocatedInUk(LocatedInUkViewModel model)
        {
            showBackButton("ServesGt10Dwellings", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

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
            showBackButton("LocatedInUk", "HeatNetwork");
            return View(new OperatingAHNViewModel());
        }

        [HttpPost]
        public IActionResult OperatingAHN(OperatingAHNViewModel model)
        {
            showBackButton("LocatedInUk", "HeatNetwork");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsExistingOrPlanned == false)
            {
                ViewBag.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
