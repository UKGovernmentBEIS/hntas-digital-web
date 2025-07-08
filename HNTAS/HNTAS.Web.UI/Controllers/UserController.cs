using Microsoft.AspNetCore.Mvc;
using HNTAS.Web.UI.Models;

namespace HNTAS.Web.UI.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult ConfirmRPIsRC()
        {
            return View(new UserModel());
        }

        [HttpPost]
        public IActionResult ConfirmRPIsRC(bool ifRPisRC)
        {
            if (ifRPisRC) {
                return RedirectToAction("ContactDetails", new UserModel());
            }
            return View("ConfirmRPIsRC", new UserModel(){ ifRPisRC = ifRPisRC });
        }

        [HttpGet]
        public IActionResult ContactDetails()
        {
            return View("ContactDetails", new UserModel());
        }
    }
}
