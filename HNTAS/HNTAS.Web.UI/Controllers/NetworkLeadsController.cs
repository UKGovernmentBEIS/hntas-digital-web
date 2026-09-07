using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models.NetworkLeads;
using HNTAS.Web.UI.Models.User;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using Microsoft.AspNetCore.Mvc;

namespace HNTAS.Web.UI.Controllers
{
    public class NetworkLeadsController : Controller
    {
        private readonly ILogger<NetworkLeadsController> _logger;
        private readonly ISessionHelper _sessionHelper;
        private readonly IInvitationService _invitationService;
        private readonly IUserService _userService;
        private readonly IInvitationTokenService _iInvitationTokenService;

        public NetworkLeadsController(ILogger<NetworkLeadsController> logger, ISessionHelper sessionHelper, IInvitationService invitationService, IUserService userService, IInvitationTokenService invitationTokenService)
        {
            _logger = logger;
            _sessionHelper = sessionHelper;
            _invitationService = invitationService;
            _userService = userService;
            _iInvitationTokenService = invitationTokenService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageLeads()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            var networkLeads = await _userService.GetNetworkLeads(userId);
            var networkLeadsToDisplay = networkLeads.Select(networkLead => new HNTAS.Web.UI.Models.User.UserDisplayModel
            {
                Name = networkLead.FirstName + " " + networkLead.LastName,
                EmailAddress = networkLead.Email,
                Status = networkLead.Status.ToString(),
                IsCurrentUser = false
            }).ToList();
            var model = new ManageLeadsModel { NetworkLeads = networkLeadsToDisplay };
            return View(model);
        }

        [HttpGet]
        public IActionResult NewLeadDetails()
        {
            ViewBag.OrganisationName = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationName);
            var model = _sessionHelper.GetFromSession<NewLeadDetailsViewModel>(HttpContext, SessionKeys.NewLeadDetailsViewModelSessionKey) ?? new NewLeadDetailsViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewLeadDetails(NewLeadDetailsViewModel model)
        {
            this.ShowBackButton("ManageLeads");
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.UserModel_Id_SessionKey);
            try
            {
                // verify here if they can take up this role or not
                var invitee = await _userService.GetUserByEmailIdAsync(model.EmailId);
                var eligibleForNetworkManager = (invitee == null) || (invitee != null && invitee.Roles.Contains(UserRole.NetworkManager));
                if (eligibleForNetworkManager)
                {
                    var invitationId = await _invitationService.AddInvitedUserAsync(
                       userId,
                       new AddInvitationRequest(
                           emailAddress: model.EmailId,
                           firstName: model.FirstName,
                           lastName: model.LastName,
                           orgId: _sessionHelper.GetFromSession<string>(HttpContext, SessionKeys.OrganisationId),
                           contributorRoles: new List<ContributorRole> { ContributorRole.NetworkManager },
                           status: InvitationStatus.Invited
                       )
                   );
                    if (string.IsNullOrWhiteSpace(invitationId))
                    {
                        TempData["ErrorMessage"] = "There was an error submitting your details. Please try again later.";
                        return RedirectToAction("ManageLeads");
                    }

                    _logger.LogInformation("Successfully submitted new organisation user details.");
                    var token = _iInvitationTokenService.GenerateToken(invitationId, model.EmailId);

                    //send invitation email
                    await _invitationService.SendInvitationEmailAsync(invitationId, new SendInvitationEmailRequest(token));
                }
                else
                {
                    TempData["ErrorMessage"] = "This user cannot be added as a Network Manager.";
                    return RedirectToAction("ManageLeads");
                }
                
            }
            catch(Exception ex)
            {
                _logger.LogError("An error occurred while processing your request. Please try again.");
                return View(model);
            }
            return RedirectToAction("NewLeadConfirmation");
        }

        [HttpGet]
        public IActionResult NewLeadConfirmation()
        {
            return View();
        }
    }
}
