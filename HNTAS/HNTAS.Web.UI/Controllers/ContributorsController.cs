using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.Components;
using HNTAS.Web.UI.Models.Contributors;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class ContributorsController : Controller
    {

        private readonly ISessionHelper _sessionHelper;
        private readonly ILogger<ContributorsController> _logger;
        private readonly IUserService _userService;
        private readonly IInvitationService _invitationService;
        private readonly IInvitationTokenService _invitationTokenService;
        private readonly IHeatNetworkService _heatNetworkService;

        public ContributorsController(ISessionHelper sessionHelper, ILogger<ContributorsController> logger, IUserService userService, IInvitationService invitationService, IInvitationTokenService invitationTokenService, IHeatNetworkService heatNetworkService)
        {
            _sessionHelper = sessionHelper;
            _logger = logger;
            _userService = userService;
            _invitationService = invitationService;
            _invitationTokenService = invitationTokenService;
            _heatNetworkService = heatNetworkService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageContributors()
        {
            _sessionHelper.ClearAllContributoFlowRelatedSessionData(HttpContext);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var managedUsers = await _userService.GetManagedUsers(userId);
            List<DDHAndContributorsListModel> listOfContributors = new List<DDHAndContributorsListModel>();
            var userRoles = await _userService.GetUserRolesAsync();

            foreach (var user in managedUsers)
            {
                var primaryRoleName = user.Roles?.FirstOrDefault();

                foreach (var heatNetwork in user.HeatNetworks)
                {
                    listOfContributors.Add(new DDHAndContributorsListModel
                    {
                        Name = user.Name,
                        HeatNetwork = heatNetwork.HnId,
                        Role = userRoles.FirstOrDefault(ur => ur.Name == primaryRoleName)?.Description,
                        Status = new InvitationStatusTag(user.Status)
                    });
                }
            }
            ViewBag.WhoDoYouWantToAdd = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey) ?? "Duty holders and contributors";
            return View(listOfContributors);
        }

        [HttpGet]
        public IActionResult NewContributorRole()
        {
            this.ShowBackButton("ManageContributors");
            var model = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey) ?? new NewContributorRoleViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewContributorRole(NewContributorRoleViewModel model)
        {
            this.ShowBackButton("ManageContributors");
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey, model);            
            return RedirectToAction("AddContributor");
        }

        private string GetRole()
        {
            string whoDoYouWantToAdd = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.WhoDoYouWantToAddSessionKey);
            if (whoDoYouWantToAdd == null)
            {
                var role = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey).IsDDH;
                whoDoYouWantToAdd = role switch
                {
                    true => "Designated duty holder",
                    false => "Contributor"
                };
            }
            else
            {
                whoDoYouWantToAdd = "Contributor";
            }
            return whoDoYouWantToAdd;
        }

        [HttpGet]
        public IActionResult AddContributor()
        {            
            ViewBag.whoDoYouWantToAdd = GetRole();
            var backAction = GetRole() == "Designated duty holder" ? "NewContributorRole" : "ManageContributors";
            this.ShowBackButton(backAction);
            var model = _sessionHelper.GetFromSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey) ?? new AddContributorViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddContributor(AddContributorViewModel model)
        {
            this.ShowBackButton("NewContributorRole");
            ViewBag.whoDoYouWantToAdd = GetRole();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            string nextAction  = model.InviteNewContributor switch
            {
                true => "NewContributorDetails",
                false => "ExistingContributorsList"
            };
            _sessionHelper.SaveToSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey, model);
            return RedirectToAction(nextAction);
        }

        [HttpGet]
        public IActionResult NewContributorDetails()
        {
            this.ShowBackButton("AddContributor");
            ViewBag.whoDoYouWantToAdd = GetRole();
            var model = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey) ?? new NewContributorDetailsViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewContributorDetails(NewContributorDetailsViewModel model)
        {
            this.ShowBackButton("AddContributor");
            ViewBag.whoDoYouWantToAdd = GetRole();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //check for rp user
            bool? isRpUser = await _userService.IsRpUserAsync(model.EmailAddress);
            if (isRpUser.HasValue && isRpUser.Value == true)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user is already registered as a Responsible Party and cannot be assigned as a contributor or Designated Duty Holder under another organisation.");
                return View(model);
            }
            // if this email address exists in the existing users list then throw error
            bool? isExistingUser = await _userService.IsActiveUserAsync(model.EmailAddress);
            if (isExistingUser.HasValue && isExistingUser.Value == true)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user already has an active account. Go back and use Add an existing user to give them access.");
                this.ShowBackButton("AddContributor");
                return View(model);
            }
            _sessionHelper.SaveToSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey, model);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "NewContributorDetails");
            return RedirectToAction("NewContributorHeatNetwork");
        }

        private async Task<List<NewContributorDetailsViewModel>> GetContributorSelectListAsync()
        {
            var userIdFromSession = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            // Call API to get list of contributors
            var contributors = await _userService.GetRegisteredUsersAsync(userIdFromSession);

            //remove hntas coordinator users
            contributors = contributors.Where(u => !u.Roles.Contains(UserRole.NetworkManager)).ToList();

            // Check for null or empty list and return an empty list if necessary
            if (contributors == null || !contributors.Any())
            {
                _logger.LogWarning("No contributors found for the current user ID {UserId}.", userIdFromSession);
                return null;
            }

            // Map contributors to a list of SelectListItem
            var selectedUsers = contributors.Select(user => new NewContributorDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailId
            }).ToList();
            _sessionHelper.SaveToSession(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey, new ExistingContributorsListViewModel { Contributors = selectedUsers });
            return selectedUsers;
        }

        [HttpGet]
        public async Task<IActionResult> ExistingContributorsList()
        {
            this.ShowBackButton("AddContributor");
            var model = _sessionHelper.GetFromSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey) ?? new ExistingContributorsListViewModel { Contributors = await GetContributorSelectListAsync() };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExistingContributorsList(ExistingContributorsListViewModel model)
        {
            this.ShowBackButton("AddContributor");
            model.Contributors = await GetContributorSelectListAsync();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            NewContributorDetailsViewModel selectedUser = model.Contributors.FirstOrDefault(x => x.EmailAddress == model.SelectedEmailAddress);
            model.SelectedUser = selectedUser;
            _sessionHelper.SaveToSession<ExistingContributorsListViewModel>(HttpContext, SessionKeys.ExistingContributorsListViewModelSessionKey, model);
            _sessionHelper.SaveToSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey, selectedUser);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "ExistingContributorsList");
            return RedirectToAction("NewContributorHeatNetwork");
        }

        private async Task<List<SelectItemOption>> GetListOfHeatNetworks()
        {
            //get heat networks from the database or service
            _logger.LogInformation("Retrieving heat networks for the user.");

            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var response = await _userService.GetUserHeatNetworks(userId);
            var heatNetworks = await Utility.GetHeatNetworkSelectListAsync(response);

            if (heatNetworks == null)
            {
                _logger.LogError("No heat networks found in API for the UserId : {UserId}", userId);
                TempData["ErrorMessage"] = "Unable to retrieve heat network information. Please try again later.";
                return null;
            }
            return heatNetworks;
        }

        [HttpGet]
        public async Task<IActionResult> NewContributorHeatNetwork()
        {
            //var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            var addContributorModel = _sessionHelper.GetFromSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey);
            var backAction = addContributorModel!.InviteNewContributor == true ? "NewContributorDetails" : "ExistingContributorsList";
            this.ShowBackButton(backAction);
            var model = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey) ?? new NewContributorHeatNetworkViewModel();
            model.HeatNetworks = await GetListOfHeatNetworks();
            if (model.HeatNetworks == null)
            {
                return RedirectToAction("Error", "Home");
            }
            else
            {
                _sessionHelper.SaveToSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey, model);
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewContributorHeatNetwork(NewContributorHeatNetworkViewModel model)
        {
            this.ShowBackButton("NewContributorDetails");
            if (!ModelState.IsValid)
            {
                model = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
                return View(model);
            }
            _sessionHelper.SaveToSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey, model);
            var network = await _heatNetworkService.GetAsync(model.SelectedHeatNetwork);
            if(network.RegistrationSource == RegistrationSource.OFGEM)
            {
                HeatNetworkPhaseViewModel phaseModel = new HeatNetworkPhaseViewModel { Phases = [], SelectedPhases = ["Operation"] };
                _sessionHelper.SaveToSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey, phaseModel);
                _sessionHelper.SaveToSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey, RegistrationSource.OFGEM);
                _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "NewContributorHeatNetwork");
                return RedirectToAction("CheckYourAnswers");
            }
            return RedirectToAction("HeatNetworkPhase");
        }

        private List<SelectItemOption> GetListOfHeatNetworkPhases()
        {
            return new List<SelectItemOption>
            {
                new SelectItemOption
                {
                    Value = "feasibility",
                    Text = "Feasibility",
                    Hint = "Concept design"
                },
                new SelectItemOption
                {
                    Value = "design",
                    Text = "Design",
                    Hint = "Developed design, technical design"
                },
                new SelectItemOption
                {
                    Value = "construction",
                    Text = "Construction",
                    Hint = "Construction design, installation, commissioning"
                },
                new SelectItemOption
                {
                    Value = "operation",
                    Text = "Operation",
                    Hint = "Operation, maintenance, ongoing monitoring"
                }
            };
        }

        [HttpGet]
        public async Task<IActionResult> HeatNetworkPhase()
        {
            this.ShowBackButton("NewContributorHeatNetwork");
            var hnModel = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            var network = await _heatNetworkService.GetAsync(hnModel.SelectedHeatNetwork);            
            HeatNetworkPhaseViewModel model;
            if (network.RegistrationSource != RegistrationSource.OFGEM) {
                model = new HeatNetworkPhaseViewModel { Phases = GetListOfHeatNetworkPhases() };
                _sessionHelper.SaveToSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey, RegistrationSource.HNTAS);
            }
            else {
                model = _sessionHelper.GetFromSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey) ?? new HeatNetworkPhaseViewModel { Phases = GetListOfHeatNetworkPhases() };
            }                
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HeatNetworkPhase(HeatNetworkPhaseViewModel model)
        {
            this.ShowBackButton("NewContributorHeatNetwork");
            model.Phases = GetListOfHeatNetworkPhases();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _sessionHelper.SaveToSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey, model);
            _sessionHelper.SaveToSession<string>(HttpContext, "backAction", "HeatNetworkPhase");
            return RedirectToAction("CheckYourAnswers");
        }

        private CheckYourAnswersViewModel CreateCYAModel()
        {
            var newContributorRoleModel = _sessionHelper.GetFromSession<NewContributorRoleViewModel>(HttpContext, SessionKeys.NewContributorRoleViewModelSessionKey);
            
            var newContributorDetailsModel = _sessionHelper.GetFromSession<NewContributorDetailsViewModel>(HttpContext, SessionKeys.NewContributorDetailsViewModelSessionKey);
            var newContributorHeatNetworkModel = _sessionHelper.GetFromSession<NewContributorHeatNetworkViewModel>(HttpContext, SessionKeys.NewContributorHeatNetworkViewModelSessionKey);
            var heatNetworkPhaseModel = _sessionHelper.GetFromSession<HeatNetworkPhaseViewModel>(HttpContext, SessionKeys.ContributorsHeatNetworkPhaseViewModelSessionKey);
            var newModel = new CheckYourAnswersViewModel
            {
                FirstName = newContributorDetailsModel?.FirstName,
                LastName = newContributorDetailsModel?.LastName,
                EmailAddress = newContributorDetailsModel?.EmailAddress,
                RoleAssigned = newContributorRoleModel?.IsDDH == true ? "designated-duty holder" : "contributor",
                HeatNetwork = newContributorHeatNetworkModel.SelectedHeatNetwork,
                SelectedPhases = heatNetworkPhaseModel?.SelectedPhases
            };
            return newModel;
        }

        [HttpGet]
        public IActionResult CheckYourAnswers()
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            this.ShowBackButton(backAction);
            ViewBag.RegistrationSource = _sessionHelper.GetFromSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey);

            var addContributorModel = _sessionHelper.GetFromSession<AddContributorViewModel>(HttpContext, SessionKeys.AddContributorViewModelSessionKey);
            ViewBag.ChangeEmailAction = addContributorModel!.InviteNewContributor == true ? "NewContributorDetails" : "ExistingContributorsList";
            var model = _sessionHelper.GetFromSession<CheckYourAnswersViewModel>(HttpContext, SessionKeys.CheckYourAnswersContributorsModelSessionKey) ?? CreateCYAModel();            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckYourAnswers(CheckYourAnswersViewModel model)
        {
            var backAction = _sessionHelper.GetFromSession<string>(HttpContext, "backAction");
            this.ShowBackButton(backAction);
            ViewBag.RegistrationSource = _sessionHelper.GetFromSession<RegistrationSource>(HttpContext, SessionKeys.RegistrationSourceKey);
            model = CreateCYAModel();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var inviteeRole = model.RoleAssigned == "contributor" ? ContributorRole.Contributor : ContributorRole.DesignatedDutyHolder;            
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                     userId,
                     new AddInvitationRequest(
                         emailAddress: model.EmailAddress,
                         firstName: model.FirstName,
                         lastName: model.LastName,
                         hnId: model.HeatNetwork,
                         contributorRoles: new List<ContributorRole> { inviteeRole },
                         replacedUserId: null,
                         rolesToReplace: new List<ContributorRole> { inviteeRole },
                         status: InvitationStatus.Invited
                     )
                 );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("CheckYourAnswers");
                }

                _logger.LogInformation("Successfully submitted new contributor details for email: {Email}", model.EmailAddress);
                var token = _invitationTokenService.GenerateToken(invitationId, model.EmailAddress);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new contributor details for email: {Email}", model.EmailAddress);
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                return RedirectToAction("CheckYourAnswers");
            }


            _sessionHelper.SaveToSession<CheckYourAnswersViewModel>(HttpContext, SessionKeys.CheckYourAnswersContributorsModelSessionKey, model);
            return RedirectToAction("UserConfirmation");
        }

        [HttpGet]
        public IActionResult UserConfirmation()
        {
            ViewBag.whoDoYouWantToAdd = GetRole();
            return View();
        }
    }
}
