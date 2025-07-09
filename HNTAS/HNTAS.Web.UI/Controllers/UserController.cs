using Microsoft.AspNetCore.Mvc;
using HNTAS.Web.UI.Models;

namespace HNTAS.Web.UI.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [Route("User/Is-user-Regulatory-Contact")]
        public IActionResult ConfirmRPIsRC()
        {
            return View(new UserModel());
        }

        [HttpPost]
        [Route("User/Is-user-Regulatory-Contact")]
        public IActionResult ConfirmRPIsRC(bool ifRPisRC)
        {
            if (ifRPisRC) {
                return RedirectToAction("ContactDetails", new UserModel());
            }
            return RedirectToAction("Guidance", "Guidance");
        }

        [HttpGet]
        public IActionResult ContactDetails()
        {
            return View("ContactDetails", new UserModel());
        }
    }
}
