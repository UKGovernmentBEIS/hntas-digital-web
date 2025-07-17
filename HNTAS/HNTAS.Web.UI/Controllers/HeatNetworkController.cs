using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Address;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class HeatNetworkController : Controller
    {
        private const string what3wordsurlModelKey = "what3wordsurl";


        [HttpGet]
        public IActionResult EnterWhat3wordsUrl()
        {
            this.ShowBackButton("EnterWhat3wordsUrl", "HeatNetwork"); // back page will be added after us-128, pointing to itself for now

            var what3wordsurlModel = SessionHelper.GetFromSession<What3wordsUrlModel>(HttpContext, what3wordsurlModelKey) ?? new What3wordsUrlModel();

            return View("EnterWhat3wordsUrl");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnterWhat3wordsUrl(What3wordsUrlModel model)
        {
            this.ShowBackButton("EnterWhat3wordsUrl", "HeatNetwork"); // back page will be added after us-128, pointing to itself for now

            if (string.IsNullOrWhiteSpace(model.what3wordsUrl))
            {
                ModelState.AddModelError(nameof(model.what3wordsUrl), "Please enter the url.");
            }
            else if (!model.what3wordsUrl.Contains("https://what3words.com/"))
            {
                ModelState.AddModelError(nameof(model.what3wordsUrl), "Invalid url. Please enter the correct url.");
            }
            else
            {
                // Extract the part after "https://what3words.com/"
                var prefix = "https://what3words.com/";
                var urlPart = model.what3wordsUrl.Substring(prefix.Length);

                // Validate: 3 words, joined by 2 dots, no whitespace
                // Regex: ^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$
                if (string.IsNullOrWhiteSpace(urlPart) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(urlPart, @"^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)$"))
                {
                    ModelState.AddModelError(nameof(model.what3wordsUrl), "Invalid url. Please enter the correct url.");
                }
            }
            
            if (!ModelState.IsValid)
            {
                // Return the view with the model to preserve user input and show errors
                return View("EnterWhat3wordsUrl", model);
            }

            SessionHelper.SaveToSession<What3wordsUrlModel>(HttpContext, what3wordsurlModelKey, model);

            return View("EnterWhat3wordsUrl", model);  // next page to be added in us-118
        }
    }
}
