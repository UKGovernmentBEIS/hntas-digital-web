using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize(Policy = SecurityConstants.Policies.CanAddHeatNetwork)]
    public class ExistingHeatNetworkRegistrationController : Controller
    {
        private readonly ISessionHelper _sessionHelper;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;
        private readonly IUserService _userService;
        private readonly ILogger<ExistingHeatNetworkRegistrationController> _logger;

        public ExistingHeatNetworkRegistrationController(ISessionHelper sessionHelper, IHeatNetworkService heatNetworkService, IOrganisationService organisationService, IUserService userService, ILogger<ExistingHeatNetworkRegistrationController> logger)
        {
            _sessionHelper = sessionHelper;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
            _userService = userService;
            _logger = logger;
        }
                

        [HttpGet]
        public async Task<IActionResult> HeatNetworkDwellingsCheck([FromQuery] string hnid)
        {            
            var hn = await _heatNetworkService.GetAsync(hnid?.ToUpper()!);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnid?.ToUpper());
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hn?.Name);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkNameModelKey, new HeatNetworkNameModel { HeatNetworkName = hn?.Name! });
            _sessionHelper.SaveToSession(HttpContext,SessionKeys.DoesHNHaveAPostcodeViewModelKey, new DoesHNHaveAPostcodeViewModel { HasPostcode = hn.Address!=null ? true : false });
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, new HeatNetworkLocationModel { HNAddressByStreet = (AddressByStreetOrTownModel)hn.Address });
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, new HeatNetworkPhaseModel { HeatNetworkPhase = "Operation" });
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkOrganisationModelKey, new HeatNetworkOrganisationModel { SelectedOrganisation = hn?.OrgId! });
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, new ECDetailsModel
            {
                LatitudeLongitude = $"{(decimal)hn.EcDetails.Latitude},{(decimal)hn.EcDetails.Longitude}",
                ECAddressByLatLong = new AddressByLatLongModel { Latitude = (decimal)hn.EcDetails.Latitude, Longitude = (decimal)hn.EcDetails.Longitude }
            });

            // set name of the controller in the view bag
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton("ExistingNetworks", "UserManagement");
            var model = _sessionHelper.GetFromSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey) ?? new HowManyDwellingsIncludedModel();
            ViewBag.HnId = hnid;
            return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkDwellingsCheck(HowManyDwellingsIncludedModel model)
        {
            this.ShowBackButton("ExistingNetworks", "UserManagement");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkDwellingsCheck", model);
            }
            _sessionHelper.SaveToSession<HowManyDwellingsIncludedModel>(HttpContext, SessionKeys.HowManyDwellingsIncludedModelKey, model);
            switch (model.HowManyDwellingsIncluded)
            {
                case "yes":
                    return RedirectToAction("HeatNetworkIntroduction");
                case "no":
                default:
                    return RedirectToAction("SixOrMoreDwellingsAnswerNo");
            }
        }

        [HttpGet]
        public IActionResult SixOrMoreDwellingsAnswerNo()
        {
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("HeatNetworkDwellingsCheck", "ExistingHeatNetworkRegistration", new { hnid = hnId, registrationSource = RegistrationSource.OFGEM.ToString() });
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/SixOrMoreDwellingsAnswerNo");
        }
        
        [HttpGet]
        public IActionResult HeatNetworkIntroduction()
        {
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            this.ShowBackButton("HeatNetworkDwellingsCheck", "ExistingHeatNetworkRegistration", new { hnid = ViewBag.HnId , registrationSource = RegistrationSource.OFGEM.ToString() });
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/HeatNetworkIntroduction");
        }    

        [HttpGet]
        public IActionResult HeatNetworkType()
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<IsHnTypeCommunalViewModel>(HttpContext, SessionKeys.IsHnTypeCommunalViewModel) ?? new IsHnTypeCommunalViewModel();
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            return View("HeatNetworkRegistration/HeatNetworkType", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkType(IsHnTypeCommunalViewModel model)
        {
            this.ShowBackButton("HeatNetworkIntroduction");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
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
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesCommunalHnHaveOwnEcViewModel) ?? new DoesCommunalHnHaveOwnEcViewModel();
            return View("HeatNetworkRegistration/HeatNetworkEcCommunal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcCommunal(DoesCommunalHnHaveOwnEcViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
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
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(HttpContext, SessionKeys.DoesCommunalEcSupplyOneBlockViewModel) ?? new DoesCommunalEcSupplyOneBlockViewModel();
            return View("HeatNetworkRegistration/HeatNetworkCommunalOneBlock", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkCommunalOneBlock(DoesCommunalEcSupplyOneBlockViewModel model)
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
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
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalECSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalECSummary");
        }

        [HttpGet]
        public IActionResult HeatNetworkCommunalOneBlockSummary()
        {
            this.ShowBackButton("HeatNetworkCommunalOneBlock");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalOneBlockSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalOneBlockSummary");
        }        

        [HttpGet]
        public IActionResult HeatNetworkCommunalNoECSummary()
        {
            this.ShowBackButton("HeatNetworkEcCommunal");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkCommunalNoECSummary");
            return View("HeatNetworkRegistration/HeatNetworkCommunalNoECSummary");
        }
        
        [HttpGet]
        public IActionResult HeatNetworkEcDistrict()
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel) ?? new DoesDistrictHnHaveOwnEcViewModel();
            return View("HeatNetworkRegistration/HeatNetworkEcDistrict", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkEcDistrict(DoesDistrictHnHaveOwnEcViewModel model)
        {
            this.ShowBackButton("HeatNetworkType");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkEcDistrict", model);
            }
            _sessionHelper.SaveToSession<DoesDistrictHnHaveOwnEcViewModel>(HttpContext, SessionKeys.DoesDistrictHnHaveOwnEcViewModel, model);
            var connectionTypesOptions = GetConnectionTypeOptions();
            var options = model.HasOwnEc == true ? connectionTypesOptions : connectionTypesOptions.Take(3);
            var connectionsTypeModel = new HeatNetworkConnectionsViewModel { Connections = options.ToList() };
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, connectionsTypeModel);
            return RedirectToAction("HeatNetworkConnections");
        }

        [HttpGet]
        public IActionResult HeatNetworkConnections()
        {
            this.ShowBackButton("HeatNetworkEcDistrict");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            return View("HeatNetworkRegistration/HeatNetworkConnections", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkConnections(HeatNetworkConnectionsViewModel model)
        {
            this.ShowBackButton("HeatNetworkEcDistrict");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
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
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey, model);
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
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkDistrictEcSummary");
            return View("HeatNetworkRegistration/HeatNetworkDistrictEcSummary", model);
        }

        [HttpGet]
        public IActionResult HeatNetworkDistrictNoEcSummary()
        {
            this.ShowBackButton("HeatNetworkConnections", "HeatNetworkRegistration");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            var model = _sessionHelper.GetFromSession<HeatNetworkConnectionsViewModel>(HttpContext, SessionKeys.HeatNetworkConnectionsViewModelKey);
            _sessionHelper.SaveToSession(HttpContext, "backActionFromHnName", "HeatNetworkDistrictNoEcSummary");
            return View("HeatNetworkRegistration/HeatNetworkDistrictNoEcSummary", model);
        }


        [HttpGet]
        public IActionResult HeatNetworkName()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton(backAction);
            var heatNetworkNameModel = _sessionHelper.GetFromSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey);
            ViewBag.HnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);            
            ViewBag.RegistrationSource = RegistrationSource.OFGEM;            
            var hnName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            heatNetworkNameModel!.HeatNetworkName = hnName!;            
            return View("HeatNetworkRegistration/HeatNetworkName", heatNetworkNameModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkName(HeatNetworkNameModel model)
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backActionFromHnName");
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            this.ShowBackButton(backAction);
            if (!ModelState.IsValid)
            {
                return View("HeatNetworkRegistration/HeatNetworkName", model);
            }
            _sessionHelper.SaveToSession<HeatNetworkNameModel>(HttpContext, SessionKeys.HeatNetworkNameModelKey, model);
            return RedirectToAction("CheckYourAnswers");
        }        

        [HttpGet]
        public async Task<IActionResult> CheckYourAnswersAsync()
        {            
            ViewBag.RegistrationSource = RegistrationSource.OFGEM;
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HnId = hnId;
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            if (heatNetworkData != null)
            {
                var phase = heatNetworkData.Phase;
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkPhaseModelKey, new HeatNetworkPhaseModel { HeatNetworkPhase = phase! });

                var lat = heatNetworkData.EcDetails?.Latitude;
                var lon = heatNetworkData.EcDetails?.Longitude;
                var ecDetails = new ECDetailsModel
                {
                    ECAddressByLatLong = new AddressByLatLongModel
                    {
                        Latitude = (decimal)lat!,
                        Longitude = (decimal)lon!,
                    },
                    LatitudeLongitude = $"{lat}, {lon}"

                };
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.ECDetailsModelSessionKey, ecDetails);

                var address = heatNetworkData.Address;
                if (address != null)
                {
                    var heatNetworkLocation = new HeatNetworkLocationModel
                    {
                        HNAddressByStreet = new AddressByStreetOrTownModel
                        {
                            StreetAddress = address.AddressLine1,
                            TownOrCity = address.Town!,
                            Postalcode = address.Postcode,
                            Country = address.Country!,
                            Fulladdress = $"{address.AddressLine1}, {address.Town}, {address.Postcode.ToUpper()}, {address.Country}"
                        }
                    };
                    _sessionHelper.SaveToSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey, heatNetworkLocation);
                }
            }
            

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

            _sessionHelper.SaveToSession<CheckYourAnswersHeatNetworkModel>(HttpContext, SessionKeys.CheckYourAnswersHeatNetworkModelKey, model);

            return View("HeatNetworkRegistration/CheckYourAnswers", model);
        }

        // Check what to add in db for type and connections


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(bool ConfirmedDeclaration)
        {
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
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

            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);

            if (hnId == null)
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
            var heatNetworkData = await _heatNetworkService.GetAsync(hnId?.ToUpper()!);
            var registrationSource = _sessionHelper.GetFromSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey);
            ViewBag.RegistrationSource = RegistrationSource.OFGEM;
            var model = new HeatNetwork
            {
                Id = heatNetworkData.Id,
                HnId = heatNetworkData.HnId,
                UHnId = heatNetworkData.UHnId,
                OrgId = heatNetworkData.OrgId,
                Name = heatNetworkData.Name,
                AdditionalDescription = viewModel?.HeatNetworkNameModel?.AdditionalDescription,
                SuppliesSixOrMoreUnits = true, // cannot create heat network, unless true
                HasAddressAndPostcode = doesHNHaveAPostcodeViewModel?.HasPostcode,
                Address = heatNetworkData.Address,
                EcDetails = heatNetworkData.EcDetails,
                HeatNetworkType = hnType,
                EcSuppliesOneCommunalBuilding = isHnTypeCommunalViewModel.IsHnTypeCommunal == true ? (doesCommunalEcSupplyOneBlockViewModel?.SuppliesOneBlock == true ? true : false) : false,
                HasOwnEnergyCenter = HasOwnEc,
                HeatNetworkConnections = heatNetworkConnections,
                Phase = heatNetworkData.Phase,
                RegistrationSource = RegistrationSource.OFGEM,
                OfgemImportedDate = heatNetworkData.OfgemImportedDate,
                OfgemUserEmailId = heatNetworkData.OfgemUserEmailId,
                CreatedAt = heatNetworkData.CreatedAt,
                CreatedBy = heatNetworkData.CreatedBy
            };

            HeatNetworkResponse heatNetworkResponse = await _heatNetworkService.RegisterOfgemNetwork(model);
            
            if (heatNetworkResponse?.HnId != null)
            {
                TempData["Confirmation_HN_Id"] = heatNetworkResponse.HnId;
                TempData["HNName"] = heatNetworkResponse.Name;
                TempData["AdditionalDescription"] = heatNetworkResponse.AdditionalDescription;
                // safe to save HnId in session at this point as it maybe used for redirection to add hn details after registration
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.HnId, heatNetworkResponse.HnId);                
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
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HnId = TempData["Confirmation_HN_Id"] as string;            
            var hnName = TempData["HNName"] as string;
            var additionalDescription = TempData["AdditionalDescription"] as string;
            ViewBag.HnName = hnName;
            ViewBag.HNNameWithDescription = hnName + (!string.IsNullOrEmpty(additionalDescription) ? ", " + additionalDescription : "");
            return View("HeatNetworkRegistration/HeatNetworkRegistrationComplete");
        }

        [HttpGet]
        public IActionResult HeatNetworkSuccessRedirection()
        {
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            ViewBag.HNId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            ViewBag.HNName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnName);
            ViewBag.RegistrationSource = RegistrationSource.OFGEM;
            var model = _sessionHelper.GetFromSession<HeatNetworkSuccessRedirection>(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey) ?? new HeatNetworkSuccessRedirection();
            return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkSuccessRedirection(HeatNetworkSuccessRedirection model)
        {
            ViewBag.ControllerName = nameof(ExistingHeatNetworkRegistrationController).Replace("Controller", string.Empty);
            if (!ModelState.IsValid) {
                return View("HeatNetworkRegistration/HeatNetworkSuccessRedirection", model);
            }
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HeatNetworkSuccessRedirectionSessionKey);
            var hnId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.HnId);
            switch (model.NextAction)
            {
                case "HNDetails":
                    return RedirectToAction("PrepareToAddNetworkDetails", "HeatNetwork", new { hnid = hnId, registrationSource = RegistrationSource.OFGEM });
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