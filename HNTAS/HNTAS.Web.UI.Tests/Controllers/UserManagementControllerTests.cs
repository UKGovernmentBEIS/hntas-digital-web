using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Workflows;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class UserManagementControllerTests
    {

        private Mock<IUrlHelper> SetUpBackLink(string controller, string action)
        {
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(u => u.Action(It.Is<UrlActionContext>(ctx =>
                    ctx.Action == action && ctx.Controller == controller)))
                .Returns($"{controller}/{action}");
            return urlHelperMock;
        }

        [Fact]
        public async Task HeatNetworksAsync_ReturnsViewWithModel_WhenUserHasHeatNetworks()
        {
            // Arrange
            var userId = "user-1";

            var mockUserService = new Mock<IUserService>();
            var mockSessionHelper = new Mock<ISessionHelper>();
            var mockLogger = new Mock<ILogger<UserManagementController>>();
            var mockWorkflowManager = new Mock<IWorkflowManager>();
            var mockHeatNetworkService = new Mock<IHeatNetworkService>();
            var mockOrganisationService = new Mock<IOrganisationService>();

            // 1. Session Mocks
            mockSessionHelper
                .Setup(s => s.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            mockSessionHelper
                .Setup(s => s.GetFromSession<DeclationOfImpartialityModel>(It.IsAny<HttpContext>(), SessionKeys.DeclarationOfImpartialityModelKey))
                .Returns((DeclationOfImpartialityModel?)null);

            // 2. Mock UserService endpoints called in Task.WhenAll
            mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(new UserResponse
            {
                Id = userId,
                Roles = new List<UserRole> { UserRole.ResponsibleParty },
                HnRoleMappings = new List<HnRoleMapping>
                {
                    new HnRoleMapping { HnId = "hn-1", Role = ContributorRole.ResponsibleParty }
                }
            });

            mockUserService.Setup(u => u.GetContributorRolesAsync()).ReturnsAsync(new List<EnumItemResponse>
            {
                new EnumItemResponse { Name = "ResponsibleParty", Description = "Responsible Party Description" }
            });

            // 3. Mock HeatNetworkService paginated response
            var paginatedResponse = new PagedResultOfUserNetworkDetailsResponse
            {
                Items = new List<UserNetworkDetailsResponse>
            {
            new UserNetworkDetailsResponse
            {
                HnId = "hn-1",
                Name = "Network 1",
                OrganisationName = "Org Ltd",
                AdditionalDescription = "Description of Network 1"
            }
            },
                PageNumber = 1,
                PageSize = 6,
                TotalCount = 1,
                TotalPages = 1
            };

            mockHeatNetworkService
                .Setup(h => h.GetHeatNetworkByUserIdPaginatedAsync(
                    userId,
                    RegistrationSource2.HNTAS,
                    1,
                    6,
                    "Name",
                    "asc"))
                .ReturnsAsync(paginatedResponse);

            // 4. Controller Setup
            var controller = new UserManagementController(
                mockUserService.Object,
                mockLogger.Object,
                mockSessionHelper.Object,
                mockWorkflowManager.Object,
                mockHeatNetworkService.Object,
                mockOrganisationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                },
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };

            controller.Url = SetUpBackLink("UserAccount", "Dashboard").Object;

            // Act
            var result = await controller.HeatNetworksAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HeatNetworksViewModel>(viewResult.Model);

            Assert.NotNull(model.HeatNetworks);
            Assert.Single(model.HeatNetworks);
            Assert.Equal("hn-1", model.HeatNetworks[0].HnId);
            Assert.Equal("Network 1", model.HeatNetworks[0].Name);
            Assert.Equal("Responsible Party Description", model.HeatNetworks[0].Role);
            Assert.True(model.IsResponsiblePerson);

            // Verify ViewBag allocations
            Assert.Equal(UserRole.ResponsibleParty.ToString(), controller.ViewBag.UserRole);
        }
    }
}
