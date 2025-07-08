using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        [HttpGet]
        public IActionResult Eligibility()
        {
            return View(new HeatNetworkEligibilityModel());
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
