using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using HNTAS.Api.Client.Api;
using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.CompaniesHouse;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IUserService _userService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IConfiguration _configuration;

        public DashboardController(ILogger<DashboardController> logger, IUserService userService, IHeatNetworkService heatNetworkService, IOrganisationService organisationService, ISessionHelper sessionHelper, IConfiguration configuration)
        {
            _logger = logger;
            _userService = userService;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
            _sessionHelper = sessionHelper;
            _configuration = configuration;
        }

        public async Task<UserDetailsResponse> RetrieveUserDetails(string userId)
        {
            try
            {
                var user = await _userService.GetUserDetails(userId);
                if (user == null)
                {
                    throw new Exception("Unable to retrieve user information. Please try again later.");
                }
                if (user.Roles != null && user.Roles.Contains(UserRole.ResponsiblePerson) && user.Organisation == null)
                {
                    throw new Exception("Your account is not associated with any organisation. Please contact support.");
                }
                return user; // Assuming you want to return user details here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user details.");
                throw; // Rethrow the exception to be handled in the calling method            
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserAccount()
        {
            _ = bool.TryParse(_configuration?.GetSection("ExistingNetworks:EnableFeature")?.Value, out bool isExistingNetworksFeatureEnabled);
            ViewBag.IsExistingNetworksFeatureEnabled = isExistingNetworksFeatureEnabled;
            UserDetailsResponse user;
            try
            {
                user = await RetrieveUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new DashboardModel());
            }            
            var isAssessorOrCertifier = "false";
            if (user.Roles[0].ToString() == HNTAS.Api.Client.Model.UserRole.Assessor.ToString() || user.Roles[0].ToString() == HNTAS.Api.Client.Model.UserRole.Certifier.ToString())
            {
                isAssessorOrCertifier = "true";
            }
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.IsAssessorOrCertifier, isAssessorOrCertifier);
            if(user.Roles[0].ToString() == HNTAS.Api.Client.Model.UserRole.DesignatedDutyHolder.ToString())
            {
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey, "Contributors");
            }
            else
            {
                _sessionHelper.SaveToSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey, null);
            }
            if (user.Organisation?.Name != null)
            {
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationName, user.Organisation.Name);
                _sessionHelper.SaveToSession(HttpContext, SessionKeys.OrganisationId, user.Organisation.OrgId);
            }

            var networks = await _heatNetworkService.GetHeatNetworkByUserId(user.Id!, RegistrationSource2.OFGEM);

            var dashboardModel = new DashboardModel
            {
                OrganisationName = user?.Organisation?.Name,
                UserRole = user.Roles[0].ToString(),
                IsResponsiblePerson = user.Roles?.Contains(UserRole.ResponsiblePerson) ?? false,
                HasHeatNetworks = user.HeatNetworks != null && user.HeatNetworks.Any(),
                HasOfgemNetworks = networks.Count != 0
            };
            var managedUsers = await _userService.GetManagedUsers(user.Id);
            if(dashboardModel.IsResponsiblePerson && managedUsers.Count <= 1 && !dashboardModel.HasHeatNetworks)
            {
                ViewBag.RPLoggedInForFirstTime = true;
            }
            else
            {
                ViewBag.RPLoggedInForFirstTime = false;
            }
            ViewBag.UserId = user.Id;
            return View(dashboardModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrganisationDetails()
        {
 
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            UserDetailsResponse user;
            bool isUserAnRP;
            try
            {
                user = await RetrieveUserDetails(_sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey));
                isUserAnRP = await _userService.IsRpUserAsync(user.EmailId) ?? false;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new OrganisationDetailsModel());
            }
            _sessionHelper.SaveToSession<string>(HttpContext, "IsUserAnRP", isUserAnRP.ToString());

            var model = new OrganisationDetailsModel
            {
                OrganisationId = user.Organisation?.OrgId,
                OrganisationName = user.Organisation?.Name,
                OrganisationType = OrganisationHelper.GetOrganisationTypeOptions().FirstOrDefault(x => x.Value == user.Organisation?.Type.ToString())?.Text,
                RPEmail = user.EmailId,
                AddressLine1 = user.Organisation?.RegisteredAddress?.AddressLine1,
                AddressLine2 = user.Organisation?.RegisteredAddress?.AddressLine2,
                Town = user.Organisation?.RegisteredAddress?.Town,
                County = user.Organisation?.RegisteredAddress?.County,
                Postcode = user.Organisation?.RegisteredAddress?.Postcode,
                Country = user.Organisation?.RegisteredAddress?.Country
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult EditOrganisationDetails()
        {
            _sessionHelper.SaveToSession<bool>(HttpContext, SessionKeys.IsEditOrganisationDetailsJourneySessionKey, true);
            return RedirectToAction("OrganisationType", "Organisation");
        }

        [HttpGet]
        public async Task<IActionResult> YourDetails()
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var user = await _userService.GetUserDetails(userId);

            Organisation org;
            OrganisationModel orgModel;
            if (user.Organisation != null)
            {
                org = await _organisationService.GetOrganisationById(user.Organisation.OrgId);
                orgModel = new OrganisationModel
                {
                    OrganisationTypes = new List<SelectListItem>(),
                    SelectedOrganisationType = org.Type.ToString(),
                    CompanyNumber = org.CompaniesHouseNumber,
                    CompanyDetails = new Models.CompaniesHouse.CompanyDetailsModel
                    {
                        Title = org.Name,
                        RegisteredOfficeAddress = new RegisteredOfficeAddressModel
                        {
                            AddressLine1 = org.RegisteredAddress.AddressLine1,
                            AddressLine2 = org.RegisteredAddress.AddressLine2,
                            Locality = org.RegisteredAddress.Town,
                            PostalCode = org.RegisteredAddress.Postcode,
                            Country = org.RegisteredAddress.Country
                        }
                    }

                };
            }
            else
            {
                org = new Organisation();
                orgModel = new OrganisationModel();
            }

            var model = new OrganisationContactDetailsModel
            {
                EmailAddress = user.EmailId,
                JobTitle = user.JobTitle,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredContactType = user.PreferredContactType == NullableOfPreferredContactType.PreferNotToSay ? PreferredContactType.PreferNotToSay : (user.PreferredContactType == NullableOfPreferredContactType.Mobile ? PreferredContactType.Mobile : PreferredContactType.Landline),
                ContactNumberExtension = user.ContactNumberExtension,
                LandlineNumber = user.LandlineNumber,
                MobileNumber = user.MobileNumber
            };
            var userModel = new UserModel
            {
                IsRegulatoryContact = user.Roles.Contains(UserRole.ResponsiblePerson),
                OrganisationName = user.Organisation?.Name,
                ContactDetails = model
            };
            
            _sessionHelper.SaveToSession<OrganisationContactDetailsModel>(HttpContext, SessionKeys.OrganisationContactDetailsModelSessionKey, model);
            _sessionHelper.SaveToSession<UserModel>(HttpContext, SessionKeys.UserCreation_SessionKey, userModel);
            _sessionHelper.SaveToSession<OrganisationModel>(HttpContext, SessionKeys.OrganisationCreation_SessionKey, orgModel);
            return View(model);
        }
    }
}
