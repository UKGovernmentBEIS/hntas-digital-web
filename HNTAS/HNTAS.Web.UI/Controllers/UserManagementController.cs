using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Authorization;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Enums;
using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace HNTAS.Web.UI.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly IHeatNetworkService _heatNetworkService;
        private readonly IOrganisationService _organisationService;
        private readonly ILogger<UserManagementController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IWorkflowManager _workflowManager;
        
        public UserManagementController(IUserService userService,
            ILogger<UserManagementController> logger,
            ISessionHelper sessionHelper,
            IWorkflowManager workflowManager,
            IHeatNetworkService heatNetworkService,
            IOrganisationService organisationService
        ){
            _logger = logger;
            _userService = userService;
            _sessionHelper = sessionHelper;
            _workflowManager = workflowManager;
            _heatNetworkService = heatNetworkService;
            _organisationService = organisationService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            try
            {
                var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);

                var users = await _userService.GetManagedUsers(userId);
                var contributorRoles = await _userService.GetContributorRolesAsync();
                var userRoles = await _userService.GetUserRolesAsync();
                var organisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                var allRoles = contributorRoles.Concat(userRoles).ToList();
                var currentUserEmailId = "";

                var displayUsers = new List<UserDisplayModel>();

                foreach (var user in users)
                {
                    if (user.Id == userId)
                    {
                        currentUserEmailId = user.EmailId;
                    }
                    displayUsers.Add(new UserDisplayModel
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailId,
                        Name = user.Name,
                        Roles = user.Roles.Select(r => allRoles.FirstOrDefault(cr => cr.Name == r.ToString()).Description).ToList(),
                        Status = user.Status.ToString(),
                        IsCurrentUser = user.Id == userId,
                        HeatNetworks = user.HeatNetworks?.Select(hn => hn.Name).ToList()
                    });
                }

                var viewModel = new ManageUsersModel
                {
                    OrganisationName = organisationName,
                    Users = displayUsers
                };

                ViewBag.ShowBackButton = true;
                ViewBag.BackLinkUrl = Url.Action("UserAccount", "Dashboard");

                return View("ManageUsers", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to manage users.");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View("ManageUsers", new ManageUsersModel());
            }
        }

        [Authorize(Policy = SecurityConstants.Policies.CanAddDDHAndContributor)]
        [HttpGet]
        public IActionResult AddContributor()
        {
            var viewModel = new AddContributorModel();
            this.ShowBackButton("UserAccount", "Dashboard");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContributor(AddContributorModel viewModel)
        {
            if (viewModel.SelectedUserType == UserType.None)
            {
                ModelState.AddModelError("SelectedUserType", "Select how you want to add a contributor.");
            }

            if (ModelState.IsValid)
            {
                if (viewModel.SelectedUserType == UserType.NewUser)
                {
                    // Initiate the workflow for adding a new contributor
                    _workflowManager.StartWorkflow<AddNewContributorWorkflowModel>(WorkflowType.AddNewContributor, ContributorWorkflowStep.AddEmailAddress);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddNewContributor);

                    return RedirectToAction("AddEmailAddress", "NewContributor");
                }
                else if (viewModel.SelectedUserType == UserType.ExistingUser)
                {
                    _workflowManager.StartWorkflow<AddExistingContributorWorkflowModel>(WorkflowType.AddExistingContributor, ExistingContributorWorkflowStep.ChooseUser);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddExistingContributor);
                    return RedirectToAction("ChooseUser", "ExistingContributor");
                }
            }

            this.ShowBackButton("UserAccount", "Dashboard");

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View(viewModel);
        }


        [HttpGet]
        public IActionResult ChangeOrganisationUser()
        {
            this.ShowBackButton("UserAccount", "Dashboard");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View();
        }

        [HttpPost]
        public IActionResult SubmitChangeOrganisationUser(OrganisationUserChangeViewModel model)
        {
            if (model.SelectedUserType == UserType.None)
            {
                ModelState.AddModelError("SelectedUserType", "Please select an option.");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                return View("ChangeOrganisationUser");
            }

            if (ModelState.IsValid)
            {
                // Redirect based on the selected option
                if (model.SelectedUserType == UserType.NewUser)
                {
                    // Initiate the workflow for adding a new contributor
                    _workflowManager.StartWorkflow<AddOrganisationUserWorkflowModel>(WorkflowType.AddOrganisationUser, AddOrganisationUserWorkflowStep.AddEmailAddress);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddOrganisationUser);

                    return RedirectToAction("AddEmailAddress", "AddOrganisationUser");

                }
                else if (model.SelectedUserType == UserType.ExistingUser)
                {
                    _workflowManager.StartWorkflow<AddExistingOrganisationUserWorkflowModel>(WorkflowType.AddExistingOrganisationUser, ExistingOrganisationUserWorkflowStep.ChooseRole);
                    _logger.LogInformation("Created new workflow state for {WorkflowType}", WorkflowType.AddExistingOrganisationUser);
                    return RedirectToAction("ChooseUser", "ExistingOrganisationUser");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworksAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 6,
            [FromQuery] string sortBy = "Name",
            [FromQuery] string sortDirection = "asc")
        {
            ClearNetworkDetailsSession();

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var user = await _userService.GetUserDetails(userId);
            var userWithHnRoles = await _userService.GetUserById(userId);
            var hnRoleMappings = userWithHnRoles?.HnRoleMappings ?? new List<HnRoleMapping>();

            ViewBag.UserRole = user?.Roles?.FirstOrDefault().ToString();
            ViewBag.HasDeclaredImpartiality = _sessionHelper.GetFromSession<DeclationOfImpartialityModel>(HttpContext, SessionKeys.DeclarationOfImpartialityModelKey)?.HasDeclaredImpartiality;

            if (user == null)
            {
                _logger.LogError("User not found in session or API.");
                TempData["ErrorMessage"] = "Unable to retrieve user information. Please try again later.";
                return View(new HeatNetworksViewModel());
            }

            // Call the paginated service method
            var paginatedResponse = await _heatNetworkService.GetHeatNetworkByUserIdPaginatedAsync(
                userId: userId,
                registrationSource: RegistrationSource2.HNTAS,
                pageNumber: pageNumber,
                pageSize: pageSize,
                sortBy: sortBy,
                sortDirection: sortDirection);

            var heatNetworks = new List<HeatNetworkModel>();

            if (paginatedResponse?.Items != null && paginatedResponse.Items.Any())
            {
                heatNetworks = (await Task.WhenAll(paginatedResponse.Items.Select(async network =>
                {
                    var org = await _organisationService.GetOrganisationById(network.OrgId);

                    return new HeatNetworkModel
                    {
                        HnId = network.HnId,
                        Name = network.Name,
                        OrganisationName = org?.Name,
                        HnDescription = network.AdditionalDescription,
                        Role = hnRoleMappings
                            .FirstOrDefault(x => x.HnId == network.HnId)?.Role.ToString() ?? "Not specified"
                    };
                }))).ToList();
            }

            var model = new HeatNetworksViewModel
            {
                HeatNetworks = heatNetworks,
                IsResponsiblePerson = user.Roles?.Contains(UserRole.ResponsiblePerson) ?? false,
                IsHntasCoordinator = user.Roles?.Contains(UserRole.NetworkManager) ?? false,

                // Pagination metadata for Razor View
                PageNumber = paginatedResponse?.PageNumber ?? pageNumber,
                PageSize = paginatedResponse?.PageSize ?? pageSize,
                TotalCount = paginatedResponse?.TotalCount ?? 0,
                TotalPages = paginatedResponse?.TotalPages ?? 0,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var isRegistrationEnabledString = Environment.GetEnvironmentVariable("IS_REGISTRATION_ENABLED");
            ViewBag.IsRegistrationEnabled = !string.IsNullOrEmpty(isRegistrationEnabledString) &&
                                             isRegistrationEnabledString.Equals("true", StringComparison.OrdinalIgnoreCase);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExistingNetworksAsync(
            string? sortBy = "ofgemImportedDate",
            string? sortOrder = "desc",
            int page = 1,
            int pageSize = 6)
        {
             
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            this.ShowBackButton("UserAccount", "Dashboard");
            _sessionHelper.ClearAllHNRegistrationFlowRelatedSessionData(HttpContext);

            try
                {
                    // Validate and sanitize inputs
                    if (page < 1) page = 1;

                    // Validate sort order
                    sortOrder = sortOrder?.ToLower() == "desc" ? "desc" : "asc";

                    var validSortFields = new[] { "EntryType", "Element" };

                    var existingNetworkRequest = new ExistingNetworkRequest
                    {
                        UserId = userId,
                        SortBy = sortBy,
                        SortDirection = sortOrder,
                        Page = page,
                        PageSize = pageSize
                    };

                    // Get audit logs with sorting and pagination
                    var result = await _heatNetworkService.GetExistingNetworkByUserId(existingNetworkRequest);

                    // Pass sorting and pagination info to view
                    ViewBag.CurrentSort = sortBy;
                    ViewBag.CurrentOrder = sortOrder;
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalPages = result.TotalPages ?? 1;
                    ViewBag.TotalItems = result.TotalCount ?? 0;                    

                    return View(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving existing networks");
                    TempData["ErrorMessage"] = "An error occurred while retrieving the existing networks.";

                    // Return empty result
                    var emptyResult = new ExistingNetworkResponse
                    {
                        Items = new List<HeatNetworkResponse>(),
                        TotalCount = 0,
                        TotalPages = 0
                    };

                    ViewBag.CurrentSort = sortBy;
                    ViewBag.CurrentOrder = sortOrder ?? "asc";
                    ViewBag.CurrentPage = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalPages = 0;
                    ViewBag.TotalItems = 0;
                    ViewBag.NextOrder = "desc";

                    return View(emptyResult);
                }

            }

        [HttpGet]
        public IActionResult ExistingNetworksAction([FromQuery] string hnId, [FromQuery] string hnName, [FromQuery] string action)
        {
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnId, hnId.ToUpper());
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.HnName, hnName);
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.RegistrationSourceKey, RegistrationSource.OFGEM);

            return RedirectToAction("HeatNetworkDwellingsCheck", "ExistingHeatNetworkRegistration", new { hnid = hnId });
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkUserRolesAsync(string hnId)
        {
            if (string.IsNullOrEmpty(hnId))
            {
                _logger.LogError("Heat network ID is null or empty in HeatNetworkUserRoles.");
                TempData["ErrorMessage"] = "Invalid heat network ID.";
                return RedirectToAction("HeatNetworks");
            }

            var heatNetworkResponse = await _heatNetworkService.GetAsync(hnId.ToUpper());

            if (heatNetworkResponse == null)
            {
                // Log the ID that wasn't found before returning BadRequest
                _logger.LogWarning("Heat network not found for ID: {HeatNetworkId}", hnId);
                return BadRequest();
            }

            var userRolesDetailsResponse = await _userService.GetHeatNetworkUserRoles(hnId.ToUpper());


            var viewModel = new HeatNetworkUserRolesViewModel
            {
                HeatNetworkName = heatNetworkResponse.Name,
                UserRoles = userRolesDetailsResponse?.Select(x => new UserRoles
                {
                    RoleName = x.RoleDescription,
                    FullName = x.FullName,
                    EmailId = x.EmailId
                }).ToList() ?? []
            };

            this.ShowBackButton("HeatNetworks");
            return View("HeatNetworkUserRoles", viewModel);
        }

        private void ClearNetworkDetailsSession()
        {
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.NetworkElementsViewModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SelectedElementsSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.ECDetailsModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.AddressByStreetOrTownModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.HeatNetworkLocationModelKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.SearchAddressByPostcodeModelSessionKey);
            _sessionHelper.ClearFromSession(HttpContext, SessionKeys.DoesHNHaveAPostcodeViewModelKey);
        }
    }
}