using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace HNTAS.Web.UI.Controllers
{
    public class AddressController : Controller
    {
        private readonly AddressLookupService _addressLookUpService;
        private readonly ILogger<AddressController> _logger;

        private const string what3wordsurlModelKey = "what3wordsurl";

        public AddressController(ILogger<AddressController> logger, AddressLookupService addressLookupService)
        {
            _logger = logger;
            _addressLookUpService = addressLookupService;
        }

        [HttpGet]
        public IActionResult SelectAddressInputMethod(string inputMethod)
        {
            if (inputMethod == "lookup")
            {
                return RedirectToAction("AddressLookUp");
            }
            else if (inputMethod == "manual")
            {
                return RedirectToAction("ManualAddressEntry");
            }
            else if (inputMethod == "noPostcode")
            {
                return RedirectToAction("NoPostcodeAddressEntry", "Address");
            }

            return View("SelectAddressInputMethod");
        }

        [HttpGet]
        public IActionResult AddressLookUp()
        {
            ModelState.Clear();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> FindAddress(string postcode)
        {
            if (string.IsNullOrEmpty(postcode))
            {
                return View("AddressLookUp");
            }
            if (!string.IsNullOrWhiteSpace(postcode) &&
                !Regex.IsMatch(postcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
            {
                ModelState.Remove("Postcode");
                ModelState.AddModelError("postcode", "Please enter a valid UK postcode.");
                return View("AddressLookUp");
            }

            try
            {
                var model = await _addressLookUpService.PostcodeLookupAsync(postcode);
                if (model == null || model.Addresses == null || model.Addresses.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                }
                return View("AddressLookUp", model);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Unable to retrieve address data.");
                return View("AddressLookUp");
            }
        }

        [HttpGet]
        public IActionResult SelectAddress(string postcode, string fulladdress, string[] addresses)
        {
            var modelIntial = new AddressLookUpModel { Postcode = postcode, Addresses = addresses };
            if (fulladdress == "helpertext")
            {
                ModelState.AddModelError("fulladress", "Address is required.");
                return View("AddressLookUp", modelIntial);
            }

            if (!string.IsNullOrWhiteSpace(postcode) &&
                !Regex.IsMatch(postcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
            {
                ModelState.Remove("Postcode");
                ModelState.AddModelError("postcode", "Please enter a valid UK postcode.");
                return View("AddressLookUp");
            }
            var model = new AddressLookUpModel
            {
                Postcode = postcode,
                Fulladdress = fulladdress
            };
            return View("SelectAddressInputMethod");
        }

        [HttpGet]
        public IActionResult ManualAddressEntry()
        {
            ModelState.Clear();
            return View("ManualAddressEntry");
        }

        [HttpPost]
        public IActionResult ManualAddressEntry(ManualAddressModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AddressLine1))
            {
                ModelState.AddModelError(nameof(model.AddressLine1), "Address line 1 is required.");
            }
            if (string.IsNullOrWhiteSpace(model.AddressTown))
            {
                ModelState.AddModelError(nameof(model.AddressTown), "Town or city is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Postcode))
            {
                ModelState.AddModelError(nameof(model.Postcode), "Postcode is required.");
            }
            if (!string.IsNullOrWhiteSpace(model.Postcode) &&
                !Regex.IsMatch(model.Postcode.Trim().ToUpper(), "^(GIR 0AA|[A-PR-UWYZ]([0-9]{1,2}|[A-HK-Y][0-9]{1,2}|[0-9][A-HJKS-UW]|[A-HK-Y][0-9][ABEHMNPRV-Y]) ?[0-9][ABD-HJLNP-UW-Z]{2})$"))
            {
                ModelState.AddModelError(nameof(model.Postcode), "Please enter a valid UK postcode.");
            }

            if (!ModelState.IsValid)
            {
                // Return the view with the model to preserve user input and show errors
                return View("ManualAddressEntry", model);
            }

            // Join non-empty fields with commas
            var addressParts = new[] { model.AddressLine1, model.AddressLine2, model.AddressTown, model.AddressCounty, model.Postcode }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);

            return View("SelectAddressInputMethod");
        }


    }
}
