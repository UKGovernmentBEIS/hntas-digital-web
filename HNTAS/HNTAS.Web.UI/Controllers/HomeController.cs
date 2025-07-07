using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Guidance() {
        return View("Guidance");
    }
}
