using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Extensions;
using HNTAS.Web.UI.Filters;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.Common;
using HNTAS.Web.UI.Models.OrganisationRole;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using HNTAS.Web.UI.Workflows.Enums;
using HNTAS.Web.UI.Workflows.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class AddOrganisationUserController : Controller
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly ISessionHelper _sessionHelper;
        private readonly IUserService _userService;
        private readonly ILogger<AddOrganisationUserController> _logger;
        private readonly IOrganisationUserService _organisationUserService;
        private readonly IInvitationService _invitationService;
        private readonly IInvitationTokenService _iInvitationTokenService;

        public AddOrganisationUserController(IWorkflowManager workflowManager,
            ISessionHelper sessionHelper,
            IUserService userService,
            ILogger<AddOrganisationUserController> logger,
            IOrganisationUserService organisationUserService,
            IInvitationService invitationService,
            IInvitationTokenService iInvitationTokenService)
        {
            _workflowManager = workflowManager;
            _sessionHelper = sessionHelper;
            _userService = userService;
            _logger = logger;
            _organisationUserService = organisationUserService;
            _invitationService = invitationService;
            _iInvitationTokenService = iInvitationTokenService;
        }

        [HttpGet]
        public IActionResult AddEmailAddress()
        {
            var state = _workflowManager.GetState<AddOrganisationUserWorkflowModel>();

            this.ShowBackButton("ChangeOrganisationUser", "UserManagement");

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            return View("Contributor/AddEmailAddress", state.Data.AddUserEmailAddressModel ?? new AddUserEmailAddressModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEmailAddress(AddUserEmailAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("Contributor/AddEmailAddress", model);
            }

            //check for rp user
            bool? isRpUser = await _userService.IsRpUserAsync(model.EmailAddress);

            if (isRpUser.HasValue && isRpUser.Value == true)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user is already registered as a Responsible Party (RP). Go back and use Add an existing user to give them access.");
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("Contributor/AddEmailAddress", model);
            }

            // if this email address exists in the existing users list then throw error
            bool? isExistingUser = await _userService.IsActiveUserAsync(model.EmailAddress);
            if (isExistingUser.HasValue && isExistingUser.Value == true)
            {
                ModelState.AddModelError(nameof(model.EmailAddress), "This user already has an active account. Go back and use Add an existing user to give them access.");
                this.ShowBackButton("AddContributor", "UserManagement");
                return View("Contributor/AddEmailAddress", model);
            }

            // Logic to save email address goes here
            _workflowManager.UpdateStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(
                m => m.AddUserEmailAddressModel = model,
                AddOrganisationUserWorkflowStep.ContactDetails
            );

            return RedirectToAction("ContactDetails");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(AddOrganisationUserWorkflowStep.ContactDetails)]
        public IActionResult ContactDetails()
        {
            var state = _workflowManager.GetState<AddOrganisationUserWorkflowModel>();
            this.ShowBackButton("AddEmailAddress");
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            return View("Contributor/ContactDetails", state.Data.ContributorContactDetailsModel ?? new ContributorContactDetailsModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveContactDetails(ContributorContactDetailsModel contactDetails)
        {
            if (!ModelState.IsValid)
            {
                this.ShowBackButton("ContactDetails");
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                TempData["ErrorSummary"] = "Custom";
                return View("Contributor/ContactDetails", contactDetails);
            }
            // Logic to save contact details goes here
            contactDetails.FirstName = contactDetails.FirstName?.Trim();
            contactDetails.LastName = contactDetails.LastName?.Trim();

            _workflowManager.UpdateStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(
                m => m.ContributorContactDetailsModel = contactDetails,
                AddOrganisationUserWorkflowStep.AssignRole
            );

            return RedirectToAction("AssignRole");
        }

        [HttpGet]
        [ValidateWorkflowStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(AddOrganisationUserWorkflowStep.AssignRole)]
        public async Task<IActionResult> AssignRole()
        {
            this.ShowBackButton("ContactDetails");

            var state = _workflowManager.GetState<AddOrganisationUserWorkflowModel>();

            List<SelectItemOption> availableRoles =
            [
                new SelectItemOption
                {
                    Value = ContributorRole.NetworkManager.ToString(),
                    Text = "Assign as a Network manager"
                },
            ];

            state.Data.RoleAssignmentModel ??= new RoleAssignmentModel();

            state.Data.RoleAssignmentModel.AvailableRoles = availableRoles;

            state.Data.RoleAssignmentModel.InvitedUserName = $"{state.Data.ContributorContactDetailsModel.FirstName} {state.Data.ContributorContactDetailsModel.LastName}";

            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);

            return View("Contributor/AssignRole", state.Data.RoleAssignmentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAssignRole(RoleAssignmentModel model)
        {
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var state = _workflowManager.GetState<AddOrganisationUserWorkflowModel>();

            //check model state error
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = [
                       new SelectItemOption
                        {
                             Value = ContributorRole.NetworkManager.ToString(),
                             Text = "Assign as a Network manager"
                        },
                    ];
                model.InvitedUserName = $"{state.Data.ContributorContactDetailsModel.FirstName} {state.Data.ContributorContactDetailsModel.LastName}";
                ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
                this.ShowBackButton("AssignRole");
                return View("Contributor/AssignRole", model);
            }

            if (state == null || state.Data == null)
            {
                _logger.LogError("Workflow state or data is null when trying to submit answers.");
                TempData["ErrorMessage"] = "Unable to submit your details. Please try again later.";
                return RedirectToAction("AssignRole");
            }

            TempData["UserName"] = $"{state.Data.ContributorContactDetailsModel?.FirstName} {state.Data.ContributorContactDetailsModel?.LastName}";
            TempData["OrganisationName"] = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            TempData["AssignedRole"] = model?.SelectedRoleName == ContributorRole.NetworkManager.ToString() ? ContributorRole.NetworkManager.ToString() : ContributorRole.ResponsibleParty.ToString();

            var selectedContributorRole = model.SelectedRoleName == ContributorRole.NetworkManager.ToString()
                ? ContributorRole.NetworkManager
                : ContributorRole.ResponsibleParty;

            var orgId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId);

            try
            {
                var invitationId = await _invitationService.AddInvitedUserAsync(
                       userId,
                       new AddInvitationRequest(
                           emailAddress: state.Data.AddUserEmailAddressModel.EmailAddress,
                           firstName: state.Data.ContributorContactDetailsModel.FirstName,
                           lastName: state.Data.ContributorContactDetailsModel.LastName,
                           contributorRoles: new List<ContributorRole> { selectedContributorRole },
                           orgId: orgId,
                           status: InvitationStatus.Invited
                       )
                   );

                if (string.IsNullOrWhiteSpace(invitationId))
                {
                    TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                    return RedirectToAction("AssignRole");
                }

                _logger.LogInformation("Successfully submitted new organisation user details.");
                var token = _iInvitationTokenService.GenerateToken(invitationId, state.Data.AddUserEmailAddressModel.EmailAddress);

                //send invitation email
                await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting new contributor details.");
                TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                return RedirectToAction("AssignRole");
            }

            _workflowManager.UpdateStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(AddOrganisationUserWorkflowStep.RoleAssignmentConfirmation);

            return RedirectToAction("RoleAssignmentConfirmation");

        }

        [HttpGet]
        [ValidateWorkflowStep<AddOrganisationUserWorkflowModel, AddOrganisationUserWorkflowStep>(AddOrganisationUserWorkflowStep.RoleAssignmentConfirmation)]
        public IActionResult RoleAssignmentConfirmation()
        {
            // Retrieve the data from TempData.
            var fullName = TempData["UserName"] as string;
            var assignedRole = TempData["AssignedRole"] as string;
            var organisationName = TempData["OrganisationName"] as string;

            // You can use a ViewBag or ViewData to pass the data to the view.
            ViewData["FullName"] = fullName;
            ViewData["AssignedRole"] = assignedRole;
            ViewData["OrganisationName"] = organisationName;

            return View("Contributor/RoleAssignmentConfirmation");
        }
    }
}
