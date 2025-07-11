using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        [HttpGet]
        public IActionResult RunningAHN()
        {
            return View(new HeatNetworkEligibilityModel());
        }


        [HttpGet]
        public IActionResult ServesGt10Dwellings()
        {
            return View();
        }

        [HttpGet]
        public IActionResult LocatedInUk()
        {
            return View();
        }

        [HttpGet]
        public IActionResult operatingAHN()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RunningAHN(HeatNetworkEligibilityModel model)
        {
            return View("ServesGt10Dwellings", model);
        }

        [HttpPost]
        public IActionResult ServesGt10Dwellings(HeatNetworkEligibilityModel model)
        {
            return View("LocatedInUk", model);
        }

        [HttpPost]
        public IActionResult LocatedInUk(HeatNetworkEligibilityModel model)
        {
            return View("operatingAHN", model);
        }

        [HttpPost]
        public IActionResult operatingAHN(HeatNetworkEligibilityModel model)
        {
            return View("ServesGt10Dwellings", model);
        }



        [HttpPost]
        public IActionResult Eligibility(HeatNetworkEligibilityModel model)
        {
            IActionResult needNotRegister()
            {
                model.ResultMessage = "You do not need to register your heat network to HNTAS.";
                return View(model);
            }

            switch (model.CurrentStep)
            {
                case 1:
                    if (model.IsRunningHeatNetwork == false) { return needNotRegister(); }
                    model.CurrentStep = 2;
                    break;
                case 2:
                    if (model.ServesMoreThan10Dwellings == false) { return needNotRegister(); }
                    model.CurrentStep = 3;
                    break;
                case 3:
                    if (model.IsInUK == false) { return needNotRegister(); }
                    model.CurrentStep = 4;
                    break;
                case 4:
                    if (model.IsExistingOrPlanned == false) { return needNotRegister(); }
                    else { model.ResultMessage = "You are eligible to register. Please create an account."; }
                    break;
            }

            return View(model);
        }
    }
}
