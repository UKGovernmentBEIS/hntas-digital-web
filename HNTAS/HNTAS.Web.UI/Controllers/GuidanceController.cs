using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class GuidanceController : Controller
    {
        [HttpGet]
        [Route("Guidance")]
        public IActionResult Guidance()
        {
            return View("Guidance");
        }
    }
}
