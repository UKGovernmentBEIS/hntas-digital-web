using Microsoft.AspNetCore.Mvc;
using System;

namespace HNTAS.Web.UI.Controllers
{
    public class GuidanceController : Controller
    {
        [HttpGet]
        [Route("Guidance")]
        public IActionResult Guidance()
        {
            ViewBag.ShowBackButton = true;
            ViewBag.BackLinkUrl = Url.Action("Guidance", "Guidance");
            return View("Guidance");
        }
    }
}
