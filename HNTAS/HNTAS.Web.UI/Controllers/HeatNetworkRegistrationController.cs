using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetwork)]
    public class HeatNetworkRegistrationController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;
        private readonly IAddressLookupService _addressLookUpService;
        private readonly IUserService _userService;
        private readonly ILogger<HeatNetworkRegistrationController> _logger;

        public HeatNetworkRegistrationController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService, IOrganisationService organisationService, IAddressLookupService addressLookupService, IUserService userService, ILogger<HeatNetworkRegistrationController> logger)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
            _addressLookUpService = addressLookupService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult HeatNetworkDwellingsCheck()
        {            
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("UserAccount", "Dashboard");

            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkDwellingsCheck(HowManyDwellingsIncludedModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("HeatNetworksAsync", "UserManagement");
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
            }
            _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    return RedirectToAction("HeatNetworkOrganisation");
                case "no":
                default:
                    return RedirectToAction("SixOrMoreDwellingsAnswerNo");
            }
        }

        [HttpGet]
        public IActionResult SixOrMoreDwellingsAnswerNo()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            return View("HeatNetworkRegistration/SixOrMoreDwellingsAnswerNo");
        }

        private async Task<List<SelectItemOption>> GetOrganisationListForUser(List<string> contributingOrganisations)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var organisationList = new List<SelectItemOption>();
            foreach (string orgId in contributingOrganisations)
            {
                var orgDetails = await _organisationService.GetOrganisationById(orgId);
                var orgEntry = orgId + " - " + orgDetails.Name;
                organisationList.Add(new SelectItemOption { Text = orgEntry, Value = orgId });
            }
            return organisationList;
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkOrganisation()
        {
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var userDetails = await _userService.GetUserById(userId);
            var contributingOrganisations = userDetails.ContributingOrganisations;
            var newModel = _sessionHelper.GetFromSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey) ?? new HeatNetworkOrganisationModel();

            if (contributingOrganisations.Count > 1)
            {
                var organisationList = await GetOrganisationListForUser(contributingOrganisations);
                newModel.OrganisationList = organisationList;
                _sessionHelper.SaveToSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey, newModel);
                _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "HeatNetworkOrganisation");
            }
            else
            {
                _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "HeatNetworkDwellingsCheck");
                var orgDetails = await _organisationService.GetOrganisationById(userDetails.OrgId);
                _sessionHelper.SaveToSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey, new HeatNetworkOrganisationModel { SelectedOrganisation = userDetails.OrgId });
                return RedirectToAction("HeatNetworkIntroduction");
            }
            var model = _sessionHelper.GetFromSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey) ?? newModel;
            return View("HeatNetworkRegistration/HeatNetworkOrganisation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HeatNetworkOrganisation(HeatNetworkOrganisationModel model)
        {
            this.ShowBackButton("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var userDetails = await _userService.GetUserById(userId);
            var contributingOrganisations = userDetails.ContributingOrganisations;
            var organisationList = await GetOrganisationListForUser(contributingOrganisations);
            model.OrganisationList = organisationList;
            _sessionHelper.SaveToSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey, model);
            if (!ModelState.IsValid)
            {                
                return View("HeatNetworkRegistration/HeatNetworkOrganisation", model);
            }
            _sessionHelper.SaveToSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey, model);
            return RedirectToAction("HeatNetworkIntroduction");
        }

        [HttpGet]
        public IActionResult HeatNetworkIntroduction()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            this.ShowBackButton(backAction);
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/HeatNetworkIntroduction");
        }    

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel) ?? new IsHnTypeCommunalViewModel();
            return View("HeatNetworkRegistration/HeatNetworkType", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkType(IsHnTypeCommunalViewModel model)
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkType", model);
            }            
            string nextAction = model.IsHnTypeCommunal switch
            {
                true => "HeatNetworkEcCommunal",
                false => "HeatNetworkEcDistrict"
            };            
            _sessionHelper.SaveToSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel, model);
            return RedirectToAction(nextAction);
        }        

        [HttpGet]
        public IActionResult HeatNetworkEcCommunal()
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel) ?? new DoesCommunalHnHaveOwnEcViewModel();
            return View("HeatNetworkRegistration/HeatNetworkEcCommunal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcCommunal(DoesCommunalHnHaveOwnEcViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkEcCommunal", model);
            }
            string nextAction = model.HasOwnEc switch
            {
                true => "HeatNetworkCommunalOneBlock",
                false => "HeatNetworkCommunalNoECSummary",
            };
            _sessionHelper.SaveToSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel, model);
            return RedirectToAction(nextAction);
        }
            

        [HttpGet]
        public IActionResult HeatNetworkCommunalOneBlock()
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel) ?? new DoesCommunalEcSupplyOneBlockViewModel();
            return View("HeatNetworkRegistration/HeatNetworkCommunalOneBlock", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkCommunalOneBlock(DoesCommunalEcSupplyOneBlockViewModel model)
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkCommunalOneBlock", model);
            }
            string nextAction = model.SuppliesOneBlock switch
            {
                true => "HeatNetworkCommunalECSummary",
                false => "HeatNetworkCommunalOneBlockSummary"
            };
            _sessionHelper.SaveToSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalECSummary()
        {
            this.ShowBackButton("HeatNetworkCommunalOneBlock");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalECSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalECSummary");
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalOneBlockSummary()
        {
            this.ShowBackButton("HeatNetworkCommunalOneBlock");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalOneBlockSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalOneBlockSummary");
        }        

        [HttpGet]
        public IActionResult HeatNetworkCommunalNoECSummary()
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalNoECSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalNoECSummary");
        }
        
        [HttpGet]
        public IActionResult HeatNetworkEcDistrict()
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel) ?? new DoesDistrictHnHaveOwnEcViewModel();
            return View("HeatNetworkRegistration/HeatNetworkEcDistrict", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcDistrict(DoesDistrictHnHaveOwnEcViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkEcDistrict", model);
            }
            _sessionHelper.SaveToSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel, model);
            var connectionTypesOptions = GetConnectionTypeOptions();
            var options = model.HasOwnEc == true ? connectionTypesOptions : connectionTypesOptions.Take(3);
            var connectionsTypeModel = new HeatNetworkConnectionsViewModel { Connections = options.ToList() };
            _sessionHelper.SaveToSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, connectionsTypeModel);
            return RedirectToAction("HeatNetworkConnections");
        }

        [HttpGet]
        public IActionResult HeatNetworkConnections()
        {
            this.ShowBackButton("HeatNetworkEcDistrict");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View("HeatNetworkRegistration/HeatNetworkConnections", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        {
            this.ShowBackButton("HeatNetworkEcDistrict");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var original = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);

            for (int i = 0; i < model.Connections.Count; i++)
            {
                model.Connections[i].Label = original.Connections[i].Label;
                model.Connections[i].HintText = original.Connections[i].HintText;
                model.Connections[i].Value = original.Connections[i].Value;
                model.Connections[i].ConditionalLabel = original.Connections[i].ConditionalLabel;
            }
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkConnections", model);
            }
            _sessionHelper.SaveToSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, model);
            var doesDistrictHnHaveOwnEcViewModel = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel);
            string nextAction = doesDistrictHnHaveOwnEcViewModel.HasOwnEc switch
            {
                true => "HeatNetworkDistrictEcSummary",
                false => "HeatNetworkDistrictNoEcSummary"
            };
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult HeatNetworkDistrictEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkDistrictEcSummary");
            return View("HeatNetworkRegistration/HeatNetworkDistrictEcSummary", model);
        }

        [HttpGet]
        public IActionResult HeatNetworkDistrictNoEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkDistrictNoEcSummary");
            return View("HeatNetworkRegistration/HeatNetworkDistrictNoEcSummary", model);
        }


        [HttpGet]
        public IActionResult HeatNetworkName()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            this.ShowBackButton(backAction);
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey) ?? new HeatNetworkNameModel();

            return View("HeatNetworkRegistration/HeatNetworkName", heatNetworkNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkName(HeatNetworkNameModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            this.ShowBackButton(backAction);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkName", model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
                        
            var isCommunalHn = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel)?.IsHnTypeCommunal ?? false;
            bool hasOwnEc = isCommunalHn
                ? (_sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel)?.HasOwnEc == true)
                : (_sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel)?.HasOwnEc == true);
            if (!isCommunalHn && !hasOwnEc)     
            {
                return RedirectToAction("ECCoordinates");
            }
            return RedirectToAction("DoesHNHaveAPostcode");
        }

        #region address input
        [HttpGet]
        public IActionResult DoesHNHaveAPostcode()
        {            
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var isCommunalHn = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel)?.IsHnTypeCommunal ?? false;
            bool hasOwnEc = isCommunalHn
                ? (_sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel)?.HasOwnEc == true)
                : (_sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel)?.HasOwnEc == true);
            var addressFor =
                isCommunalHn && hasOwnEc ? "energy centre" :
                isCommunalHn && !hasOwnEc ? "communal network" :
                !isCommunalHn && hasOwnEc ? "main energy centre" : "";
            _sessionHelper.SaveToSession<string>(HttpContext, "addressFor", addressFor);
            ViewBag.addressFor = addressFor;
            var model = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey) ?? new DoesHNHaveAPostcodeViewModel();
            return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoesHNHaveAPostcode(DoesHNHaveAPostcodeViewModel model)
        {
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");

            if (!ModelState.IsValid)
            {                
                return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
            }            
            if ((bool)!model.HasPostcode!)
            {
                model.Postcode = null;
                _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey, model);
                _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, null);
                _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, null);
                _sessionHelper.SaveToSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey, null);
                return RedirectToAction("ECCoordinates");
            }
            else
            {
                SearchAddressByPostcodeModel results = await _addressLookUpService.PostcodeLookupAsync(model.Postcode);
                model.Postcode = model.Postcode?.ToUpperInvariant().Trim();
                if (results == null || results.Addresses == null || results.Addresses.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve address data for this postcode.");
                    return View("HeatNetworkRegistration/DoesHNHaveAPostcode", model);
                }
                results.Addresses = results.Addresses
                    .Select(address => Utility.CapitalizeCommaSeparated(address))
                    .ToArray();
                _sessionHelper.SaveToSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey, model);
                _sessionHelper.SaveToSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey, results);

                return RedirectToAction("SearchByPostcodeResults");
            }            
        }

        [HttpGet]
        public IActionResult SearchByPostcodeResults()
        {
            this.ShowBackButton("DoesHNHaveAPostcode");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            SearchAddressByPostcodeModel model = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (model == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel is null.");
                return View("HeatNetworkRegistration/DoesHNHaveAPostcode");
            }
            return View("HeatNetworkRegistration/SearchByPostcodeResults", model);
        }

        [HttpGet]
        public IActionResult SelectAddress(string selectedAddress)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var addressmodel = _sessionHelper.GetFromSession<SearchAddressByPostcodeModel>(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);

            if (addressmodel == null)
            {
                _logger.LogError("SearchAddressByPostcodeModel not found in session.");
                return BadRequest("Session expired or invalid. Please try again.");
            }
            addressmodel.SelectedFullAddress = Utility.CapitalizeCommaSeparated(selectedAddress);
            var addressParts = addressmodel.SelectedFullAddress.Split(",");
            if (addressParts.Length < 3)
            {
                var sanitizedAddress = selectedAddress?
                    .Replace("\r", " ")
                    .Replace("\n", " ");
                _logger.LogError("Malformed address received: {Address}", sanitizedAddress);
                return BadRequest("Selected address is not in the expected format. It must contain at least street, town/city, and postcode.");
            }

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = string.Join(",", addressParts.Take(addressParts.Length - 2)).Trim() ?? string.Empty,
                TownOrCity = addressParts[addressParts.Length - 2].Trim() ?? string.Empty,
                Postalcode = (addressParts[addressParts.Length - 1]).ToUpper().Trim() ?? string.Empty,
                Country = "United Kingdom" ?? string.Empty,
                Fulladdress = addressmodel.SelectedFullAddress
            };
            // Save the new model to session
            _sessionHelper.SaveToSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey, model);
            return RedirectToAction("ConfirmAddress");
        }

        [HttpGet]
        public IActionResult AddressManualEntry()
        {
            this.ShowBackButton("HeatNetworkName", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            var model = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey)?.HNAddressByStreet ?? new AddressByStreetOrTownModel { Country = "United Kingdom" };
            return View("HeatNetworkRegistration/AddressManualEntry", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddressManualEntry(AddressByStreetOrTownModel model)
        {
            this.ShowBackButton("HeatNetworkName");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/AddressManualEntry", model);
            }
            var addressParts = new[] { model.StreetAddress, model.TownOrCity, model.Postalcode.ToUpper(), model.Country }
                .Where(part => !string.IsNullOrWhiteSpace(part));
            model.Fulladdress = string.Join(", ", addressParts);
            var heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel();
            heatNetworkLocationModel.HNAddressByStreet = model;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);

            return RedirectToAction("ECCoordinates");
        }

        [HttpGet]
        public IActionResult ConfirmAddress()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            return View("HeatNetworkRegistration/ConfirmAddress", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmAddress(AddressByStreetOrTownModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return RedirectToAction("SaveHNAddressByPostcode");
        }
        #endregion

        [HttpGet]
        public IActionResult SaveHNAddressByPostcode()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var model = _sessionHelper.GetFromSession<AddressByStreetOrTownModel>(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey) ?? new AddressByStreetOrTownModel();            
            var doesHnHaveAPostcodeModel = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel;
            if (doesHnHaveAPostcodeModel?.HasPostcode == true)
            {
                heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey) ?? new HeatNetworkLocationModel { HNAddressByStreet = new AddressByStreetOrTownModel() };
                heatNetworkLocationModel.HNAddressByStreet = model;
            }
            else
            {
                heatNetworkLocationModel = null;
            }           
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocationModel);
            return RedirectToAction("ECCoordinates");
        }

        #region Coordinates input
        [HttpGet]
        public IActionResult ECCoordinates()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var isCommunalHn = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel)?.IsHnTypeCommunal ?? false;
            bool hasOwnEc = isCommunalHn
                ? (_sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel)?.HasOwnEc == true)
                : (_sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel)?.HasOwnEc == true);
            this.ShowBackButton(!isCommunalHn && !hasOwnEc ? "HeatNetworkName" : "DoesHNHaveAPostcode");
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            ViewBag.QuestionForWithEC = "What are the energy centre's grid coordinates?";
            ViewBag.QuestionForWithoutEC = "What are the grid coordinates for your communal network?";
            ViewBag.WithEc = hasOwnEc;
            var model = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey) ?? new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel() };
            return View("HeatNetworkRegistration/ECCoordinates", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ECCoordinates(ECDetailsModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var isCommunalHn = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel)?.IsHnTypeCommunal ?? false;
            bool hasOwnEc = isCommunalHn
                ? (_sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel)?.HasOwnEc == true)
                : (_sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel)?.HasOwnEc == true);         
            this.ShowBackButton(!isCommunalHn && !hasOwnEc ? "HeatNetworkName" : "DoesHNHaveAPostcode");
            ViewBag.addressFor = _sessionHelper.GetFromSession<string>(HttpContext, "addressFor");
            ViewBag.QuestionForWithEC = "What are the energy centre's grid coordinates?";
            ViewBag.QuestionForWithoutEC = "What are the grid coordinates for your communal network?";
            ViewBag.WithEc = hasOwnEc;
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/ECCoordinates", model);
            }

            // Try to split and parse the LatitudeLongitude value
            var raw = model.LatitudeLongitude;
            var parts = raw.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);

            if (parts.Length != 2
                || !decimal.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lat)
                || !decimal.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var lon))
            {
                ModelState.AddModelError(nameof(model.LatitudeLongitude),
                    "Enter latitude and longitude in correct format.");
                return View("HeatNetworkRegistration/ECCoordinates", model);
            }

            // Populate nested AddressByLatLongModel
            model.ECAddressByLatLong.Latitude = lat;
            model.ECAddressByLatLong.Longitude = lon;
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, model);
            return RedirectToAction("HeatNetworkPhase");
        }
        #endregion

        

        [HttpGet]
        public IActionResult HeatNetworkPhase()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("ECCoordinates");
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey) ?? new HeatNetworkPhaseModel();            
            return View("HeatNetworkRegistration/HeatNetworkPhase", heatNetworkPhaseModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseModel model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("ECCoordinates");
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkPhase", model);
            }
            else if (string.IsNullOrWhiteSpace(model.HeatNetworkPhase))
            {
                ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                return View("HeatNetworkRegistration/HeatNetworkPhase", model);
            }
            else
            {
                _sessionHelper.SaveToSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, model);
                switch (model.HeatNetworkPhase)
                {
                    case "Feasibility":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "1" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");
                    case "Design":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "2" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");                    
                    case "Construction":
                        _sessionHelper.SaveToSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey, new PathwayModel() { Pathway = "3" });
                        return RedirectToAction("CheckYourAnswers", "HeatNetworkRegistration");                    
                    default:
                        ModelState.AddModelError(nameof(model.HeatNetworkPhase), "Please select a valid heat network phase.");
                        return View("HeatNetworkRegistration/HeatNetworkPhase", model);
                }
            }
        }

        [HttpGet]
        public IActionResult HeatNetworkLeaveService()
        {
            this.ShowBackButton("HeatNetworkPhase");
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/HeatNetworkLeaveService");
        }

        [HttpGet]
        public async Task<IActionResult> CheckYourAnswersAsync()
        {            
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.ShowBackButton = false;
            HowManyDwellingsIncludedModel howManyDwellingsIncludedModel = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey);
            HeatNetworkOrganisationModel heatNetworkOrganisationModel = _sessionHelper.GetFromSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey);
            HeatNetworkNameModel heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            ECDetailsModel ecDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            HeatNetworkPhaseModel heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);
            IsHnTypeCommunalViewModel isHnTypeCommunalViewModel = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel);
            DoesCommunalHnHaveOwnEcViewModel doesCommunalHnHaveOwnEcViewModel = _sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel);
            DoesDistrictHnHaveOwnEcViewModel doesDistrictHnHaveOwnEcViewModel = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel);
            DoesCommunalEcSupplyOneBlockViewModel doesCommunalEcSupplyOneBlockViewModel = _sessionHelper.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel);
            HeatNetworkConnectionsViewModel heatNetworkConnectionsModel = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
                        

            if (heatNetworkNameModel == null || heatNetworkPhaseModel == null || isHnTypeCommunalViewModel == null || (isHnTypeCommunalViewModel.IsHnTypeCommunal == false && heatNetworkConnectionsModel == null))
            {
                return RedirectToAction("UserAccount", "Dashboard");
            }

            var model = new CheckYourAnswersHeatNetworkModel
            {
                DoesHnHaveMoreThan6Dwellings = howManyDwellingsIncludedModel.HowManyDwellingsIncluded == "yes" ? "Yes" : "No",
                OrgId = heatNetworkOrganisationModel.SelectedOrganisation,
                HeatNetworkType = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? "Communal" : "District",
                ECSuppliesOneCommunalBuilding = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? (doesCommunalEcSupplyOneBlockViewModel?.SuppliesOneBlock == true ? "Yes" : "No") : null,
                HasOwnEnergyCenter = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? (doesCommunalHnHaveOwnEcViewModel?.HasOwnEc == true ? "Yes" : "No, it does not have its own energy centre") : (doesDistrictHnHaveOwnEcViewModel?.HasOwnEc == true ? "Yes" : "No, it does not have its own main energy centre"),
                HeatNetworkConnectionsModel = heatNetworkConnectionsModel,
                ECDetailsModel = ecDetailsModel,
                HeatNetworkNameModel = heatNetworkNameModel,
                HeatNetworkAddressModel = heatNetworkLocationModel?.HNAddressByStreet ?? null,
                HeatNetworkPhaseModel = heatNetworkPhaseModel,
                PathwayModel = _sessionHelper.GetFromSession<PathwayModel>(HttpContext, SessionKeys.PathwayModelKey) ?? new PathwayModel() { Pathway = "1" },
                ConfirmedDeclaration = false
            };

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;

            _sessionHelper.SaveToSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey, model);

            return View("HeatNetworkRegistration/CheckYourAnswers", model);
        }

        // Check what to add in db for type and connections


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(bool ConfirmedDeclaration)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            var viewModel = _sessionHelper.GetFromSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey);

            HowManyDwellingsIncludedModel howManyDwellingsIncludedModel = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey);
            HeatNetworkOrganisationModel heatNetworkOrganisationModel = _sessionHelper.GetFromSession<HeatNetworkOrganisationModel>(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey);
            IsHnTypeCommunalViewModel isHnTypeCommunalViewModel = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel);
            DoesCommunalHnHaveOwnEcViewModel doesCommunalHnHaveOwnEcViewModel = _sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel);
            DoesDistrictHnHaveOwnEcViewModel doesDistrictHnHaveOwnEcViewModel = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel);
            DoesCommunalEcSupplyOneBlockViewModel doesCommunalEcSupplyOneBlockViewModel = _sessionHelper.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel);
            HeatNetworkConnectionsViewModel heatNetworkConnectionsModel = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);

            HeatNetworkNameModel heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            DoesHNHaveAPostcodeViewModel doesHNHaveAPostcodeViewModel = _sessionHelper.GetFromSession<DoesHNHaveAPostcodeViewModel>(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
            HeatNetworkLocationModel heatNetworkLocationModel = _sessionHelper.GetFromSession<HeatNetworkLocationModel>(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            ECDetailsModel ecDetailsModel = _sessionHelper.GetFromSession<ECDetailsModel>(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            HeatNetworkPhaseModel heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseModel>(HttpContext, SessionKeys.HeatNetworkPhaseModelKey);

            ModelState.Clear();

            // Validate the mandatory checkbox
            if (ConfirmedDeclaration != true)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmedDeclaration), "You must confirm the declaration to proceed.");
            }

            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            if (userId == null || orgId == null)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }
            HNTAS.Api.Client.Model.HeatNetworkType hnType = isHnTypeCommunalViewModel.IsHnTypeCommunal switch
            {
                true => HNTAS.Api.Client.Model.HeatNetworkType.Communal,
                false => HNTAS.Api.Client.Model.HeatNetworkType.District
            };            
            bool HasOwnEc = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? doesCommunalHnHaveOwnEcViewModel.HasOwnEc == true : doesDistrictHnHaveOwnEcViewModel.HasOwnEc == true;
            HNTAS.Api.Client.Model.HeatNetworkConnections heatNetworkConnections = null;
            if (isHnTypeCommunalViewModel.IsHnTypeCommunal == false)
            {
                heatNetworkConnections = new HNTAS.Api.Client.Model.HeatNetworkConnections();
                foreach (var connection in heatNetworkConnectionsModel.Connections)
                {
                    if(connection.IsSelected && connection.Value == ConnectionType.CommunalBuildings.ToString())
                    {
                        heatNetworkConnections.IsCommunalBuilding = true;
                        heatNetworkConnections.NoOfCommunalBuilding = connection.ConditionalValue;
                    }else if (connection.IsSelected && connection.Value == ConnectionType.IndividualHomes.ToString())
                    {
                        heatNetworkConnections.IsDomesticConsumer = true;
                        heatNetworkConnections.NoOfDomesticConsumer = connection.ConditionalValue;
                    }else if (connection.IsSelected && connection.Value == ConnectionType.CommercialConnection.ToString())
                    {
                        heatNetworkConnections.IsNonDomesticConsumer = true;
                        heatNetworkConnections.NoOfNonDomesticConsumer = connection.ConditionalValue;
                    }else if (connection.IsSelected && connection.Value == ConnectionType.OtherDistrictNetwork.ToString())
                    {
                        heatNetworkConnections.IsOtherDistrictNetwork = true;
                        heatNetworkConnections.NoOfOtherDistrictNetwork = connection.ConditionalValue;
                    }
                }                
            }
            
            var hnAddress = viewModel?.HeatNetworkAddressModel;

            double? latitude = null;
            double? longitude = null;
            if (viewModel?.ECDetailsModel?.ECAddressByLatLong != null)
            {
                latitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Latitude;
                longitude = (double?)viewModel.ECDetailsModel.ECAddressByLatLong.Longitude;
            }

            ECDetails? ecDetails = (latitude.HasValue || longitude.HasValue)
                ? new ECDetails(latitude: latitude, longitude: longitude)
                : null;

            var address = viewModel.HeatNetworkAddressModel != null ? new RegisteredAddress(
                    addressLine1: hnAddress?.StreetAddress?.Trim(),
                    postcode: hnAddress?.Postalcode?.Trim(),
                    addressLine2: default,
                    town: hnAddress?.TownOrCity?.Trim(),
                    county: default,
                    country: hnAddress?.Country?.Trim()
                ) : null;
            
            ViewBag.RegistrationSource = RegistrationSource.HNTAS;

            var model = new HeatNetwork
            {
                OrgId = heatNetworkOrganisationModel.SelectedOrganisation,
                Name = viewModel?.HeatNetworkNameModel?.HeatNetworkName,
                AdditionalDescription = viewModel?.HeatNetworkNameModel?.AdditionalDescription,
                SuppliesSixOrMoreUnits = true, // cannot create heat network, unless true
                HasAddressAndPostcode = doesHNHaveAPostcodeViewModel?.HasPostcode,
                Address = address,
                EcDetails = ecDetails,
                HeatNetworkType = hnType,
                EcSuppliesOneCommunalBuilding = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? (doesCommunalEcSupplyOneBlockViewModel?.SuppliesOneBlock == true ? true : false) : false,
                HasOwnEnergyCenter = HasOwnEc,
                HeatNetworkConnections = heatNetworkConnections,
                Pathway = viewModel?.PathwayModel?.Pathway,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                RegistrationSource = RegistrationSource.HNTAS,
                Phase = viewModel?.HeatNetworkPhaseModel?.HeatNetworkPhase
            };

            HeatNetworkResponse heatNetworkResponse;

            heatNetworkResponse = await _heatNetworkService.AddHeatNetwork(model);            

            if (heatNetworkResponse?.HnId != null)
            {
                await _organisationService.UpdateOrgHeatNetworkId(orgId, userId, heatNetworkResponse.HnId);
                TempData["Confirmation_HN_Id"] = heatNetworkResponse.HnId;
                TempData["HNName"] = heatNetworkResponse.Name;
                TempData["AdditionalDescription"] = heatNetworkResponse.AdditionalDescription;
                // safe to save HnId in session at this point as it maybe used for redirection to add hn details after registration
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnId, heatNetworkResponse.HnId);
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnName, heatNetworkResponse.Name);
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your heat network details. Please try again later.";
                return View("HeatNetworkRegistration/CheckYourAnswers", viewModel);
            }
            _sessionHelper.ClearAllHNRegistrationFlowRelatedSessionData(HttpContext);
            _sessionHelper.SetIsCheckAnswerFlow(HttpContext, false);            
            return RedirectToAction("HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkRegistrationComplete()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HNId = TempData["Confirmation_HN_Id"] as string;
            var hnName = TempData["HNName"] as string;
            var additionalDescription = TempData["AdditionalDescription"] as string;
            ViewBag.HNNameWithDescription = hnName + (!string.IsNullOrEmpty(additionalDescription) ? ", " + additionalDescription : "");
            return View("HeatNetworkRegistration/HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public IActionResult HeatNetworkSuccessRedirection()
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HNId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HNName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);            
            ViewBag.RegistrationSource = RegistrationSource.HNTAS;
            var model = _sessionHelper.GetFromSession<HeatNetworkSuccessRedirection>(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey) ?? new HeatNetworkSuccessRedirection();
            return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkSuccessRedirection(HeatNetworkSuccessRedirection model)
        {
            ViewBag.ControllerName = nameof(HeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid) {
                return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
            }
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            switch (model.NextAction)
            {
                case "HNDetails":
                    return RedirectToAction("AddNetworkDetails", "HeatNetwork", new { hnid = hnId });
                case "AddHN":
                    return RedirectToAction("HeatNetworkDwellingsCheck", "HeatNetworkRegistration");
                case "Dashboard":
                default:
                    return RedirectToAction("UserAccount", "Dashboard");
            }
        }


        // Utility functions
        private List<HeatNetworkConnectionCheckboxItem> GetConnectionTypeOptions()
        {
            return new List<HeatNetworkConnectionCheckboxItem>
            {
                new() {
                    Label = "All communal buildings (including those you don't own)",
                    HintText = "Multiple consumers in a residential, office, commercial or mixed-use building",
                    Value = ConnectionType.CommunalBuildings.ToString(),
                    IsSelected = false,
                    ConditionalLabel = "Number of communal buildings",
                    ConditionalValue = null          
                },
                new() {
                    Label = "Individual homes",
                    HintText = "Houses or houses divided into individual flats",
                    Value = ConnectionType.IndividualHomes.ToString(),
                    IsSelected = false,
                    ConditionalLabel = "Number of individual homes",
                    ConditionalValue = null
                },
                new() {
                    Label = "Non-domestic buildings or consumers",
                    Value = ConnectionType.CommercialConnection.ToString(),
                    HintText = "Buildings such as offices, hotels, schools or retail units",
                    IsSelected = false,
                    ConditionalLabel = "Number of non-domestic buildings",
                    ConditionalValue = null
                },
                new() {
                    Label = "Other district heat networks supplied by this network",
                    Value = ConnectionType.OtherDistrictNetwork.ToString(),
                    HintText = "As your network has a main energy centre, you could be supplying other district networks",
                    IsSelected = false,
                    ConditionalLabel = "Number of other district networks",
                    ConditionalValue = null
                }
            };
        }
    }
}