using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.Address;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class HeatNetworkRegistrationControllerTests
    {
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly Mock<IAddressLookupService> _addressLookupServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<HeatNetworkRegistrationController>> _loggerMock;

        private readonly HeatNetworkRegistrationController _controller;

        public HeatNetworkRegistrationControllerTests()
        {
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _addressLookupServiceMock = new Mock<IAddressLookupService>();
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<HeatNetworkRegistrationController>>();
            _controller = CreateController();
        }

        private Mock<IUrlHelper> SetUpBackLink(string action)
        {
            return TestingUtility.SetUpBackLink("HeatNetworkRegistration", action);
        }

        private HeatNetworkRegistrationController CreateController()
        {
            var controller = new HeatNetworkRegistrationController(
                _sessionHelperMock.Object,
                _heatNetworkServiceMock.Object,
                _organisationServiceMock.Object,
                _addressLookupServiceMock.Object,
                _userServiceMock.Object,
                _loggerMock.Object
            );
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            controller.Url = new Mock<IUrlHelper>().Object;
            return controller;
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Get_ReturnsView_WithSessionModel()
        {
            // Arrange
            var expectedModel = new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(expectedModel);
            _controller.Url = TestingUtility.SetUpBackLink("UserManagement", "HeatNetworks").Object;

            // Act
            var result = _controller.HeatNetworkDwellingsCheck();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(expectedModel, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Get_ReturnsView_WithNewModel_WhenSessionNull()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns((HowManyDwellingsIncludedModel)null);

            // Act
            var result = _controller.HeatNetworkDwellingsCheck();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HowManyDwellingsIncludedModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_ValidYes_RedirectsToOrganisation()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel
            {
                HowManyDwellingsIncluded = "yes"
            };
            _controller.Url = TestingUtility.SetUpBackLink("UserManagement", "HeatNetworks").Object;

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HowManyDwellingsIncludedModelKey,
                model), Times.Once);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkOrganisation", redirect.ActionName);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel();
            _controller.ModelState.AddModelError("HowManyDwellingsIncluded", "Required");

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<HowManyDwellingsIncludedModel>()),
                Times.Never);
        }

        [Fact]
        public void SixOrMoreDwellingsAnswerNo_Get_ReturnsView()
        {
            // Act
            var result = _controller.SixOrMoreDwellingsAnswerNo();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void SixOrMoreDwellingsAnswerNo_Get_DoesNotRedirect()
        {
            // Act
            var result = _controller.SixOrMoreDwellingsAnswerNo();

            // Assert
            Assert.IsNotType<RedirectToActionResult>(result);
        }
        
        [Fact]
        public async Task HeatNetworkOrganisation_Get_MultipleOrgs_ReturnsView()
        {
            // Arrange
            var userId = "user1";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _userServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync(new UserResponse
                {
                    ContributingOrganisations = new List<string> { "org1", "org2" }
                });

            _organisationServiceMock.Setup(x => x.GetOrganisationById(It.IsAny<string>()))
                .ReturnsAsync(new Organisation { Name = "Org" });

            // Act
            var result = await _controller.HeatNetworkOrganisation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task HeatNetworkOrganisation_Get_SingleOrg_Redirects()
        {
            // Arrange
            var userId = "user1";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _userServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync(new UserResponse
                {
                    OrgId = "org1",
                    ContributingOrganisations = new List<string> { "org1" }
                });

            _organisationServiceMock.Setup(x => x.GetOrganisationById("org1"))
                .ReturnsAsync(new Organisation { Name = "Org One" });

            // Act
            var result = await _controller.HeatNetworkOrganisation();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkIntroduction", redirect.ActionName);
        }

        [Fact]
        public async Task HeatNetworkOrganisation_Post_Valid_Redirects()
        {
            // Arrange
            var userId = "user1";

            var model = new HeatNetworkOrganisationModel
            {
                SelectedOrganisation = "org1"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _userServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync(new UserResponse
                {
                    ContributingOrganisations = new List<string> { "org1", "org2" }
                });

            _organisationServiceMock.Setup(x => x.GetOrganisationById(It.IsAny<string>()))
                .ReturnsAsync(new Organisation { Name = "Org" });

            // Act
            var result = await _controller.HeatNetworkOrganisation(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkIntroduction", redirect.ActionName);
        }

        [Fact]
        public async Task HeatNetworkOrganisation_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var userId = "user1";

            var model = new HeatNetworkOrganisationModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("SelectedOrganisation", "Required");

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                SessionKeys.UserModel_Id_SessionKey))
                .Returns(userId);

            _userServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync(new UserResponse
                {
                    ContributingOrganisations = new List<string> { "org1" }
                });

            _organisationServiceMock.Setup(x => x.GetOrganisationById(It.IsAny<string>()))
                .ReturnsAsync(new Organisation { Name = "Org" });

            // Act
            var result = await _controller.HeatNetworkOrganisation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }
        
        [Fact]
        public void HeatNetworkIntroduction_Get_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backAction"))
                .Returns("HeatNetworkOrganisation");

            // Act
            var result = _controller.HeatNetworkIntroduction();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkIntroduction_Get_NoBackAction_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backAction"))
                .Returns((string)null);

            // Act
            var result = _controller.HeatNetworkIntroduction();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkType_Get_WithSessionModel_ReturnsView()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel { IsHnTypeCommunal = true };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkType();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkType_Get_NoSession_ReturnsNewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns((IsHnTypeCommunalViewModel)null);

            // Act
            var result = _controller.HeatNetworkType();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<IsHnTypeCommunalViewModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkType_Post_ValidTrue_RedirectsToEcCommunal()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkEcCommunal", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkType_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel();

            _controller.ModelState.AddModelError("IsHnTypeCommunal", "Required");

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<IsHnTypeCommunalViewModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkType_Post_ValidFalse_RedirectsToEcDistrict()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkEcDistrict", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel,
                model), Times.Once);
        }        

        [Fact]
        public void HeatNetworkEcCommunal_Get_WithSessionModel_ReturnsView()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkEcCommunal();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Get_NoSession_ReturnsNewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns((DoesCommunalHnHaveOwnEcViewModel)null);

            // Act
            var result = _controller.HeatNetworkEcCommunal();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DoesCommunalHnHaveOwnEcViewModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_ValidTrue_RedirectsToOneBlock()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkCommunalOneBlock", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_ValidFalse_RedirectsToNoEcSummary()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkCommunalNoECSummary", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel();

            _controller.ModelState.AddModelError("HasOwnEc", "Required");

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<DoesCommunalHnHaveOwnEcViewModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Get_WithSessionModel_ReturnsView()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalEcSupplyOneBlockViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Get_NoSession_ReturnsNewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalEcSupplyOneBlockViewModel))
                .Returns((DoesCommunalEcSupplyOneBlockViewModel)null);

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DoesCommunalEcSupplyOneBlockViewModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_ValidTrue_RedirectsToECSummary()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkCommunalECSummary", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalEcSupplyOneBlockViewModel,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_ValidFalse_RedirectsToOneBlockSummary()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkCommunalOneBlockSummary", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalEcSupplyOneBlockViewModel,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel();

            _controller.ModelState.AddModelError("SuppliesOneBlock", "Required");

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            Assert.IsType<ViewResult>(result);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<DoesCommunalEcSupplyOneBlockViewModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkCommunalECSummary_Get_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkCommunalECSummary();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkCommunalECSummary_Get_SavesBackAction()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            _controller.HeatNetworkCommunalECSummary();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkCommunalECSummary"), Times.Once);
        }        

        [Fact]
        public void HeatNetworkCommunalOneBlockSummary_Get_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkCommunalOneBlockSummary();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlockSummary_Get_SavesBackAction()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            _controller.HeatNetworkCommunalOneBlockSummary();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkCommunalOneBlockSummary"), Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalNoECSummary_Get_ReturnsView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkCommunalNoECSummary();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HeatNetworkCommunalNoECSummary_Get_SavesBackAction()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            _controller.HeatNetworkCommunalNoECSummary();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkCommunalNoECSummary"), Times.Once);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Get_WithSessionModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkEcDistrict();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkEcDistrict", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Get_NoSession_ReturnsNewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns((DoesDistrictHnHaveOwnEcViewModel)null);

            // Act
            var result = _controller.HeatNetworkEcDistrict();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkEcDistrict", viewResult.ViewName);
            Assert.IsType<DoesDistrictHnHaveOwnEcViewModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel();

            _controller.ModelState.AddModelError("HasOwnEc", "Required");

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<DoesDistrictHnHaveOwnEcViewModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_ValidTrue_SavesAndRedirects()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnections", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel,
                model), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey,
                It.IsAny<HeatNetworkConnectionsViewModel>()),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_ValidFalse_UsesLimitedOptions()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkConnections", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel,
                model), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey,
                It.IsAny<HeatNetworkConnectionsViewModel>()),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkConnections_Get_ReturnsViewWithModel()
        {
            // Arrange
            var model = new HeatNetworkConnectionsViewModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkConnections();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkConnections_Get_NoSession_ReturnsNullModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns((HeatNetworkConnectionsViewModel)null);

            // Act
            var result = _controller.HeatNetworkConnections();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkConnections_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var original = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem { Label = "A", HintText = "H", Value = "1", ConditionalLabel = "C" }
                }
            };

            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem() // empty, will be overwritten
                }
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(original);

            _controller.ModelState.AddModelError("Connections", "Invalid");

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            // verify mapping happened
            Assert.Equal("A", model.Connections[0].Label);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<HeatNetworkConnectionsViewModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkConnections_Post_ValidTrue_RedirectsToEcSummary()
        {
            // Arrange
            var originalConnections = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem { Label = "A", HintText = "H", Value = "1", ConditionalLabel = "C" }
                }
            };

            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem()
                }
            };

            var districtModel = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(originalConnections);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(districtModel);

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDistrictEcSummary", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkConnections_Post_ValidFalse_RedirectsToNoEcSummary()
        {
            // Arrange
            var originalConnections = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem { Label = "A", HintText = "H", Value = "1", ConditionalLabel = "C" }
                }
            };

            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new HeatNetworkConnectionCheckboxItem()
                }
            };

            var districtModel = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(originalConnections);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(districtModel);

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkDistrictNoEcSummary", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkDistrictEcSummary_Get_ReturnsViewWithModel()
        {
            // Arrange
            var model = new HeatNetworkConnectionsViewModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkDistrictEcSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDistrictEcSummary_Get_NoSession_ReturnsNullModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns((HeatNetworkConnectionsViewModel)null);

            // Act
            var result = _controller.HeatNetworkDistrictEcSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkDistrictEcSummary"), Times.Once);
        }

        [Fact]
        public void HeatNetworkDistrictNoEcSummary_Get_SavesBackAction()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            _controller.HeatNetworkDistrictNoEcSummary();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkDistrictNoEcSummary"), Times.Once);
        }

        [Fact]
        public void HeatNetworkDistrictNoEcSummary_Get_NoSession_ReturnsNullModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns((HeatNetworkConnectionsViewModel)null);

            // Act
            var result = _controller.HeatNetworkDistrictNoEcSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "backActionFromHnName",
                "HeatNetworkDistrictNoEcSummary"), Times.Once);
        }

        [Fact]
        public void HeatNetworkName_Get_WithSessionModel_ReturnsView()
        {
            // Arrange
            var model = new HeatNetworkNameModel { HeatNetworkName = "Test" };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns("SomeAction");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkNameModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkName();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }
        

        [Fact]
        public void HeatNetworkName_Get_NoSession_ReturnsNewModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns((string)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkNameModelKey))
                .Returns((HeatNetworkNameModel)null);

            // Act
            var result = _controller.HeatNetworkName();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HeatNetworkNameModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkName_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new HeatNetworkNameModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("HeatNetworkName", "Required"); // ✅ moved here

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns("SomeAction");

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                It.IsAny<HeatNetworkNameModel>()),
                Times.Never);
        }

        [Fact]
        public void HeatNetworkName_Post_Valid_Redirects()
        {
            // Arrange
            var model = new HeatNetworkNameModel
            {
                HeatNetworkName = "Valid Name"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkNameModelKey,
                model), Times.Once);
        }

        [Fact]
        public void HeatNetworkName_Post_CommunalWithEc_RedirectsToPostcode()
        {
            // Arrange
            var model = new HeatNetworkNameModel();

            var communal = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = true
            };

            var ecModel = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns("SomeAction");

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(communal);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(ecModel);

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DoesHNHaveAPostcode", redirect.ActionName);
        }

        [Fact]
        public void HeatNetworkName_Post_DistrictWithEc_RedirectsToEcCoordinates()
        {
            // Arrange
            var model = new HeatNetworkNameModel();

            var communal = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = false
            };

            var ecModel = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns("SomeAction");

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(communal);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(ecModel);

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("DoesHNHaveAPostcode", redirect.ActionName);
        }

        [Fact]
        public void HeatNetworkName_Post_DistrictWithoutEc_RedirectsToPostcode()
        {
            // Arrange
            var model = new HeatNetworkNameModel();

            var communal = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = false
            };

            var ecModel = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "backActionFromHnName"))
                .Returns("SomeAction");

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(communal);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(ecModel);

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_CommunalWithEc_SetsEnergyCentre()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = true });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = true });

            // Act
            var result = _controller.DoesHNHaveAPostcode();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("energy centre", _controller.ViewBag.addressFor);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                "addressFor",
                "energy centre"), Times.Once);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_CommunalNoEc_SetsCommunalNetwork()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = true });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = false });

            var result = _controller.DoesHNHaveAPostcode();

            Assert.Equal("communal network", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_DistrictWithEc_SetsMainEnergyCentre()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = false });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = true });

            var result = _controller.DoesHNHaveAPostcode();

            Assert.Equal("main energy centre", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_DistrictNoEc_SetsEmpty()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = false });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = false });

            var result = _controller.DoesHNHaveAPostcode();

            Assert.Equal("", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_NullSession_DefaultsToEmpty()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns((IsHnTypeCommunalViewModel)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns((DoesDistrictHnHaveOwnEcViewModel)null);

            var result = _controller.DoesHNHaveAPostcode();

            Assert.Equal("", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void DoesHNHaveAPostcode_Get_NoViewModelSession_ReturnsNewModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey))
                .Returns((DoesHNHaveAPostcodeViewModel)null);

            var result = _controller.DoesHNHaveAPostcode();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<DoesHNHaveAPostcodeViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new DoesHNHaveAPostcodeViewModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("HasPostcode", "Required");

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            // Act
            var result = await _controller.DoesHNHaveAPostcode(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_NoPostcode_RedirectsToEcCoordinates()
        {
            // Arrange
            var model = new DoesHNHaveAPostcodeViewModel
            {
                HasPostcode = false
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            // Act
            var result = await _controller.DoesHNHaveAPostcode(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey,
                It.IsAny<DoesHNHaveAPostcodeViewModel>()), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession<object>(
                It.IsAny<HttpContext>(),
                It.IsAny<string>(),
                null), Times.AtLeast(2));
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_PostcodeLookupReturnsNull_ReturnsViewWithError()
        {
            // Arrange
            var model = new DoesHNHaveAPostcodeViewModel
            {
                HasPostcode = true,
                Postcode = "AB1 2CD"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            _addressLookupServiceMock.Setup(x => x.PostcodeLookupAsync(It.IsAny<string>()))
                .ReturnsAsync((SearchAddressByPostcodeModel)null);

            // Act
            var result = await _controller.DoesHNHaveAPostcode(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_NoAddressesFound_ReturnsViewWithError()
        {
            // Arrange
            var model = new DoesHNHaveAPostcodeViewModel
            {
                HasPostcode = true,
                Postcode = "AB1 2CD"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            _addressLookupServiceMock.Setup(x => x.PostcodeLookupAsync(It.IsAny<string>()))
                .ReturnsAsync(new SearchAddressByPostcodeModel
                {
                    Addresses = [] // empty
                });

            // Act
            var result = await _controller.DoesHNHaveAPostcode(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task DoesHNHaveAPostcode_Post_ValidPostcode_RedirectsToSearchResults()
        {
            // Arrange
            var model = new DoesHNHaveAPostcodeViewModel
            {
                HasPostcode = true,
                Postcode = "ab12cd"
            };

            var results = new SearchAddressByPostcodeModel
            {
                Addresses = new List<string> { "addr1", "addr2" }.ToArray()
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            _addressLookupServiceMock.Setup(x => x.PostcodeLookupAsync(It.IsAny<string>()))
                .ReturnsAsync(results);

            // Act
            var result = await _controller.DoesHNHaveAPostcode(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SearchByPostcodeResults", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey,
                model), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey,
                It.IsAny<SearchAddressByPostcodeModel>()), Times.Once);
        }

        [Fact]
        public void SearchByPostcodeResults_Get_WithModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SearchAddressByPostcodeModel
            {
                Addresses = new List<string> { "Addr1" }.ToArray()
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(model);

            // Act
            var result = _controller.SearchByPostcodeResults();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal("energy centre", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void SearchByPostcodeResults_Get_NoModel_LogsError_AndReturnsFallbackView()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns((SearchAddressByPostcodeModel)null);

            // Act
            var result = _controller.SearchByPostcodeResults();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/DoesHNHaveAPostcode", viewResult.ViewName);

            // ✅ verify logger called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("is null")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SearchByPostcodeResults_Get_SetsViewBagAddressFor()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor"))
                .Returns("communal network");

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns((SearchAddressByPostcodeModel)null);

            // Act
            _controller.SearchByPostcodeResults();

            // Assert
            Assert.Equal("communal network", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void SelectAddress_Post_NoSession_ReturnsBadRequest()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns((SearchAddressByPostcodeModel)null);

            // Act
            var result = _controller.SelectAddress("Some address");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Session expired", badRequest.Value.ToString());

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SelectAddress_Post_InvalidAddressFormat_ReturnsBadRequest()
        {
            // Arrange
            var sessionModel = new SearchAddressByPostcodeModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(sessionModel);

            var input = "OnlyStreet"; // < 3 parts

            // Act
            var result = _controller.SelectAddress(input);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("expected format", badRequest.Value.ToString());

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Malformed address")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SelectAddress_Post_ValidAddress_ParsesAndRedirects()
        {
            // Arrange
            var sessionModel = new SearchAddressByPostcodeModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<SearchAddressByPostcodeModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.SearchAddressByPostcodeModelSessionKey))
                .Returns(sessionModel);

            var input = "123 Street, Town, AB12CD";

            // Act
            var result = _controller.SelectAddress(input);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ConfirmAddress", redirect.ActionName);            
        }

        [Fact]
        public void AddressManualEntry_Get_WithSessionModel_ReturnsViewWithModel()
        {
            // Arrange
            var hnLocation = new HeatNetworkLocationModel
            {
                HNAddressByStreet = new AddressByStreetOrTownModel
                {
                    StreetAddress = "123 Road",
                    TownOrCity = "Town",
                    Country = "United Kingdom"
                }
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey)).Returns(hnLocation);

            // Act
            var result = _controller.AddressManualEntry();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddressByStreetOrTownModel>(viewResult.Model);

            Assert.Equal("123 Road", model.StreetAddress);
            Assert.Equal("energy centre", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void AddressManualEntry_Get_NoSession_ReturnsDefaultModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("communal network");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey))
                .Returns((HeatNetworkLocationModel)null);

            // Act
            var result = _controller.AddressManualEntry();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AddressByStreetOrTownModel>(viewResult.Model);

            Assert.Equal("United Kingdom", model.Country);
        }

        [Fact]
        public void AddressManualEntry_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("StreetAddress", "Required");

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("energy centre");

            // Act
            var result = _controller.AddressManualEntry(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void AddressManualEntry_Post_ValidModel_SavesAndRedirects()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "123 Road",
                TownOrCity = "Town",
                Postalcode = "AB1 2CD",
                Country = "United Kingdom"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey))
                .Returns((HeatNetworkLocationModel)null); // force new

            // Act
            var result = _controller.AddressManualEntry(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet.Fulladdress.Contains("123 Road") &&
                    m.HNAddressByStreet.Postalcode == "AB1 2CD")), Times.Once);
        }

        [Fact]
        public void AddressManualEntry_Post_ExistingSession_UpdatesModel()
        {
            // Arrange
            var existing = new HeatNetworkLocationModel();

            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "New Street",
                TownOrCity = "City",
                Postalcode = "ZZ1 1ZZ",
                Country = "United Kingdom"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey))
                .Returns(existing);

            // Act
            var result = _controller.AddressManualEntry(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet.StreetAddress == "New Street")), Times.Once);
        }

        [Fact]
        public void AddressManualEntry_Post_BuildsFullAddress_WithoutEmptyParts()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "123 Road",
                TownOrCity = "",
                Postalcode = "AB12CD",
                Country = "United Kingdom"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(),
                "addressFor")).Returns("energy centre");

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey))
                .Returns((HeatNetworkLocationModel)null);

            // Act
            _controller.AddressManualEntry(model);

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet.Fulladdress == "123 Road, AB12CD, United Kingdom")), Times.Once);
        }

        [Fact]
        public void ConfirmAddress_Get_WithSessionModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel
            {
                StreetAddress = "123 Road",
                TownOrCity = "Town",
                Postalcode = "AB12CD"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns(model);

            // Act
            var result = _controller.ConfirmAddress();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void ConfirmAddress_Get_NoSession_ReturnsNullModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns((AddressByStreetOrTownModel)null);

            // Act
            var result = _controller.ConfirmAddress();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public void ConfirmAddress_Post_RedirectsToSaveHNAddressByPostcode()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel();

            // Act
            var result = _controller.ConfirmAddress(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SaveHNAddressByPostcode", redirect.ActionName);
        }

        [Fact]
        public void ConfirmAddress_Post_InvalidModel_StillRedirects()
        {
            // Arrange
            var model = new AddressByStreetOrTownModel();

            _controller.ModelState.AddModelError("StreetAddress", "Required");

            // Act
            var result = _controller.ConfirmAddress(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SaveHNAddressByPostcode", redirect.ActionName);
        }

        [Fact]
        public void SaveHNAddressByPostcode_Get_HasPostcodeTrue_CreatesNewLocation()
        {
            // Arrange
            var address = new AddressByStreetOrTownModel { StreetAddress = "123 Road" };
            var postcodeModel = new DoesHNHaveAPostcodeViewModel { HasPostcode = true };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey)).Returns(address);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey)).Returns(postcodeModel);

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey)).Returns((HeatNetworkLocationModel)null);

            // Act
            var result = _controller.SaveHNAddressByPostcode();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ECCoordinates", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet == address)), Times.Once);
        }

        [Fact]
        public void SaveHNAddressByPostcode_Get_HasPostcodeTrue_UpdatesExisting()
        {
            // Arrange
            var address = new AddressByStreetOrTownModel { StreetAddress = "New Street" };
            var existing = new HeatNetworkLocationModel();

            var postcodeModel = new DoesHNHaveAPostcodeViewModel { HasPostcode = true };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey)).Returns(address);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey)).Returns(postcodeModel);

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey)).Returns(existing);

            // Act
            var result = _controller.SaveHNAddressByPostcode();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet == address)), Times.Once);
        }

        [Fact]
        public void SaveHNAddressByPostcode_Get_HasPostcodeFalse_ClearsLocation()
        {
            // Arrange
            var postcodeModel = new DoesHNHaveAPostcodeViewModel { HasPostcode = false };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey)).Returns(postcodeModel);

            // Act
            var result = _controller.SaveHNAddressByPostcode();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            _sessionHelperMock.Verify(x => x.SaveToSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                null), Times.Once);
        }

        [Fact]
        public void SaveHNAddressByPostcode_Get_NoAddress_SetsEmptyModel()
        {
            // Arrange
            var postcodeModel = new DoesHNHaveAPostcodeViewModel { HasPostcode = true };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<AddressByStreetOrTownModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.AddressByStreetOrTownModelSessionKey))
                .Returns((AddressByStreetOrTownModel)null);

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesHNHaveAPostcodeViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesHNHaveAPostcodeViewModelKey)).Returns(postcodeModel);

            // Act
            _controller.SaveHNAddressByPostcode();

            // Assert
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey,
                It.Is<HeatNetworkLocationModel>(m =>
                    m.HNAddressByStreet != null)), Times.Once);
        }

        [Fact]
        public void ECCoordinates_Get_CommunalWithEc_SetsCorrectBackAndFlag()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel{ IsHnTypeCommunal = true });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = true });

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(), "addressFor"))
                .Returns("energy centre");

            // Act
            var result = _controller.ECCoordinates();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True((bool)_controller.ViewBag.WithEc);
            Assert.Equal("energy centre", _controller.ViewBag.addressFor);
        }

        [Fact]
        public void ECCoordinates_Get_CommunalNoEc_SetsWithEcFalse()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = true });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = false });

            var result = _controller.ECCoordinates();

            Assert.False((bool)_controller.ViewBag.WithEc);
        }

        [Fact]
        public void ECCoordinates_Get_DistrictWithEc_SetsWithEcTrue()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = false });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = true });

            var result = _controller.ECCoordinates();

            Assert.True((bool)_controller.ViewBag.WithEc);
        }

        [Fact]
        public void ECCoordinates_Get_DistrictNoEc_SetsWithEcFalse()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = false });

            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = false });

            var result = _controller.ECCoordinates();

            Assert.False((bool)_controller.ViewBag.WithEc);
        }

        [Fact]
        public void ECCoordinates_Get_NullSession_DefaultsToFalse()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns((IsHnTypeCommunalViewModel)null);

            var result = _controller.ECCoordinates();

            Assert.False((bool)_controller.ViewBag.WithEc);
        }

        [Fact]
        public void ECCoordinates_Get_WithSessionModel_ReturnsModel()
        {
            var model = new ECDetailsModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<ECDetailsModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.ECDetailsModelSessionKey))
                .Returns(model);

            var result = _controller.ECCoordinates();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void ECCoordinates_Get_NoModel_ReturnsDefaultModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<ECDetailsModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.ECDetailsModelSessionKey))
                .Returns((ECDetailsModel)null);

            var result = _controller.ECCoordinates();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ECDetailsModel>(viewResult.Model);

            Assert.NotNull(model.ECAddressByLatLong);
        }

        [Fact]
        public void ECCoordinates_Post_InvalidModel_ReturnsView()
        {
            // Arrange
            var model = new ECDetailsModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("LatitudeLongitude", "Required");

            _sessionHelperMock.Setup(x => x.GetFromSession<string>(
                It.IsAny<HttpContext>(), "addressFor"))
                .Returns("energy centre");

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void ECCoordinates_Post_InvalidFormat_ReturnsViewWithError()
        {
            // Arrange
            var model = new ECDetailsModel
            {
                LatitudeLongitude = "123.45" // only one part
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/ECCoordinates", viewResult.ViewName);

            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("LatitudeLongitude", _controller.ModelState.Keys);
        }

        [Fact]
        public void ECCoordinates_Post_NonNumericValues_ReturnsError()
        {
            // Arrange
            var model = new ECDetailsModel
            {
                LatitudeLongitude = "abc, xyz"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void ECCoordinates_Post_ValidCoordinates_SavesAndRedirects()
        {
            // Arrange
            var model = new ECDetailsModel
            {
                LatitudeLongitude = "51.5074, -0.1278",
                ECAddressByLatLong = new AddressByLatLongModel()
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkPhase", redirect.ActionName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.ECDetailsModelSessionKey,
                It.Is<ECDetailsModel>(m =>
                    m.ECAddressByLatLong.Latitude == 51.5074m &&
                    m.ECAddressByLatLong.Longitude == -0.1278m)), Times.Once);
        }

        [Fact]
        public void ECCoordinates_Post_ValidCoordinates_WithSpaces_ParsesCorrectly()
        {
            // Arrange
            var model = new ECDetailsModel
            {
                LatitudeLongitude = " 51.5 ,  -0.12 ",
                ECAddressByLatLong = new AddressByLatLongModel()
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            // ✅ Redirect happened
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkPhase", redirect.ActionName);

            // ✅ Values parsed correctly (trim + split worked)
            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.ECDetailsModelSessionKey,
                It.Is<ECDetailsModel>(m =>
                    m.ECAddressByLatLong.Latitude == 51.5m &&
                    m.ECAddressByLatLong.Longitude == -0.12m)), Times.Once);
        }

        [Fact]
        public void ECCoordinates_Post_EmptyInput_ReturnsError()
        {
            // Arrange
            var model = new ECDetailsModel
            {
                LatitudeLongitude = ""
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.ECCoordinates(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void HeatNetworkPhase_Get_WithSessionModel_ReturnsView()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel
            {
                HeatNetworkPhase = "Feasibility"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkPhaseModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkPhase();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/HeatNetworkPhase", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkPhase_Get_NoSession_ReturnsNewModel()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkPhaseModelKey))
                .Returns((HeatNetworkPhaseModel)null);

            var result = _controller.HeatNetworkPhase();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HeatNetworkPhaseModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkPhase_Post_InvalidModelState_ReturnsView()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.ModelState.AddModelError("HeatNetworkPhase", "Required");

            // Act
            var result = _controller.HeatNetworkPhase(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkPhase_Post_EmptyValue_ReturnsError()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel
            {
                HeatNetworkPhase = ""
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkPhase(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public void HeatNetworkPhase_Post_Feasibility_RedirectsAndSetsPathway1()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel
            {
                HeatNetworkPhase = "Feasibility"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkPhase(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("CheckYourAnswers", redirect.ActionName);
            Assert.Equal("HeatNetworkRegistration", redirect.ControllerName);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.PathwayModelKey,
                It.Is<PathwayModel>(p => p.Pathway == "1")), Times.Once);
        }

        [Fact]
        public void HeatNetworkPhase_Post_InvalidValue_ReturnsError()
        {
            // Arrange
            var model = new HeatNetworkPhaseModel
            {
                HeatNetworkPhase = "InvalidValue"
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.HeatNetworkPhase(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);

            Assert.Contains(nameof(model.HeatNetworkPhase), _controller.ModelState.Keys);
        }

        [Fact]
        public async Task CheckYourAnswers_Get_MissingCriticalData_RedirectsToDashboard()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkNameModelKey))
                .Returns((HeatNetworkNameModel)null); // triggers guard

            // Act
            var result = await _controller.CheckYourAnswersAsync();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserAccount", redirect.ActionName);
            Assert.Equal("Dashboard", redirect.ControllerName);
        }

        [Fact]
        public async Task CheckYourAnswers_Get_CommunalWithEc_BuildsCorrectModel()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            SetupAllValidSessionData(isCommunal: true, hasOwnEc: true);

            // Act
            var result = await _controller.CheckYourAnswersAsync();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(view.Model);

            Assert.Equal("Communal", model.HeatNetworkType);
            Assert.Equal("Yes", model.HasOwnEnergyCenter);
        }

        [Fact]
        public async Task CheckYourAnswers_Get_CommunalNoEc_SetsCorrectText()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            SetupAllValidSessionData(isCommunal: true, hasOwnEc: false);

            var result = await _controller.CheckYourAnswersAsync();

            var model = Assert.IsType<CheckYourAnswersHeatNetworkModel>(
                ((ViewResult)result).Model);

            Assert.Equal("No, it does not have its own energy centre", model.HasOwnEnergyCenter);
        }

        [Fact]
        public async Task CheckYourAnswers_Get_SavesModelToSession()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            SetupAllValidSessionData(isCommunal: true, hasOwnEc: true);

            await _controller.CheckYourAnswersAsync();

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.CheckYourAnswersHeatNetworkModelKey,
                It.IsAny<CheckYourAnswersHeatNetworkModel>()),
                Times.Once);
        }        

        private void SetupAllValidSessionData(bool isCommunal = true, bool hasOwnEc = true)
        {
            _sessionHelperMock.Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(new HowManyDwellingsIncludedModel { HowManyDwellingsIncluded = "yes" });

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkNameModelKey))
                .Returns(new HeatNetworkNameModel { HeatNetworkName = "Hn" });

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkPhaseModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkPhaseModelKey))
                .Returns(new HeatNetworkPhaseModel { HeatNetworkPhase = "Feasibility" });

            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = isCommunal });

            
            if (isCommunal)
            {
                _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                    .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = hasOwnEc });
            }
            else
            {
                _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                    .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = hasOwnEc });

                _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                    .Returns(new HeatNetworkConnectionsViewModel());
            }               

            

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkOrganisationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkOrganisationModelKey))
                .Returns(new HeatNetworkOrganisationModel { SelectedOrganisation = "org1" });

            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkLocationModel>(
                It.IsAny<HttpContext>(),
                SessionKeys.HeatNetworkLocationModelKey))
                .Returns(new HeatNetworkLocationModel
                {
                    HNAddressByStreet = new AddressByStreetOrTownModel
                    {
                        StreetAddress = "123 St",
                        TownOrCity = "town",
                        Postalcode = "AB12CD",
                        Country = "United Kingdom",
                        Fulladdress = "123 St, town, AB12CD"
                    }                    
                });
        }

        [Fact]
        public async Task SubmitAnswers_ReturnsViewWithModelError_WhenDeclarationNotConfirmed()
        {
            // Arrange
            var viewModel = new CheckYourAnswersHeatNetworkModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(viewModel);

            // Act
            var result = await _controller.SubmitAnswers(false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Same(viewModel, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey(nameof(viewModel.ConfirmedDeclaration)));
        }

        [Fact]
        public async Task SubmitAnswers_ReturnsViewWithError_WhenUserIdOrOrgIdIsNull()
        {
            // Arrange
            var viewModel = new CheckYourAnswersHeatNetworkModel();
            _sessionHelperMock.Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(viewModel);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns((string)null);
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId)).Returns((string)null);

            // Act
            var result = await _controller.SubmitAnswers(true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Same(viewModel, viewResult.Model);
            Assert.Equal("An error occurred while submitting your heat network details. Please try again later.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task SubmitAnswers_ReturnsViewWithError_WhenAddHeatNetworkReturnsNull()
        {
            // Arrange
            SetupValidSessionForSubmitAnswers();
            _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>())).ReturnsAsync((HeatNetworkResponse)null);

            // Act
            var result = await _controller.SubmitAnswers(true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Equal("An error occurred while submitting your heat network details. Please try again later.", _controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task SubmitAnswers_SavesHnIdAndRedirects_WhenAddHeatNetworkSucceeds()
        {
            // Arrange
            SetupValidSessionForSubmitAnswers();
            var hnResponse = new HeatNetworkResponse(hnId: "hn123", name: "HN Name", additionalDescription: "desc");
            _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>())).ReturnsAsync(hnResponse);

            // Act
            var result = await _controller.SubmitAnswers(true);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("HeatNetworkRegistrationComplete", redirect.ActionName);
            _organisationServiceMock.Verify(x => x.UpdateOrgHeatNetworkId(It.IsAny<string>(), It.IsAny<string>(), "hn123"), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnId, "hn123"), Times.Once);
            _sessionHelperMock.Verify(x => x.SaveToSession<string>(It.IsAny<HttpContext>(), SessionKeys.HnName, "HN Name"), Times.Once);
            Assert.Equal("hn123", _controller.TempData["Confirmation_HN_Id"]);
            Assert.Equal("HN Name", _controller.TempData["HNName"]);
            Assert.Equal("desc", _controller.TempData["AdditionalDescription"]);
        }        

        [Fact]
        public async Task SubmitAnswers_HeatNetworkConnections_IsNull_ForCommunal()
        {
            // Arrange
            SetupValidSessionForSubmitAnswers(isCommunal: true);
            HeatNetwork capturedModel = null;
            _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()))
                .Callback<HeatNetwork>(hn => capturedModel = hn)
                .ReturnsAsync(new HeatNetworkResponse(hnId: "hn123", name: "HN Name", additionalDescription: "desc"));

            // Act
            await _controller.SubmitAnswers(true);

            // Assert
            Assert.Null(capturedModel.HeatNetworkConnections);
        }

        [Fact]
        public async Task SubmitAnswers_HeatNetworkConnections_IsSet_ForDistrict()
        {
            // Arrange
            SetupValidSessionForSubmitAnswers(isCommunal: false);
            HeatNetwork capturedModel = null;
            _heatNetworkServiceMock.Setup(x => x.AddHeatNetwork(It.IsAny<HeatNetwork>()))
                .Callback<HeatNetwork>(hn => capturedModel = hn)
                .ReturnsAsync(new HeatNetworkResponse(hnId: "hn123", name: "HN Name", additionalDescription: "desc"));

            // Act
            await _controller.SubmitAnswers(true);

            // Assert
            Assert.NotNull(capturedModel.HeatNetworkConnections);
            Assert.True(capturedModel.HeatNetworkConnections.IsCommunalBuilding);
            Assert.True(capturedModel.HeatNetworkConnections.IsDomesticConsumer);
            Assert.True(capturedModel.HeatNetworkConnections.IsNonDomesticConsumer);
            Assert.True(capturedModel.HeatNetworkConnections.IsOtherDistrictNetwork);
        }

        private void SetupValidSessionForSubmitAnswers(bool isCommunal = false)
        {
            var viewModel = new CheckYourAnswersHeatNetworkModel
            {
                HeatNetworkNameModel = new HeatNetworkNameModel { HeatNetworkName = "HN Name", AdditionalDescription = "desc" },
                HeatNetworkAddressModel = new AddressByStreetOrTownModel { StreetAddress = "Street", Postalcode = "PC", TownOrCity = "Town", Country = "UK" },
                ECDetailsModel = new ECDetailsModel { ECAddressByLatLong = new AddressByLatLongModel { Latitude = 1, Longitude = 2 } },
                PathwayModel = new PathwayModel { Pathway = "1" },
                HeatNetworkPhaseModel = new HeatNetworkPhaseModel { HeatNetworkPhase = "Feasibility" }
            };
            _sessionHelperMock.Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(It.IsAny<HttpContext>(), SessionKeys.CheckYourAnswersHeatNetworkModelKey)).Returns(viewModel);
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkOrganisationModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkOrganisationModelKey))
                .Returns(new HeatNetworkOrganisationModel { SelectedOrganisation = "org1" });
            _sessionHelperMock.Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(It.IsAny<HttpContext>(), SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel { IsHnTypeCommunal = isCommunal });
            _sessionHelperMock.Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(It.IsAny<HttpContext>(), SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel { HasOwnEc = true });
            _sessionHelperMock.Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(It.IsAny<HttpContext>(), SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel { HasOwnEc = true });
            _sessionHelperMock.Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(It.IsAny<HttpContext>(), SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(new HeatNetworkConnectionsViewModel
                {
                    Connections = new List<HeatNetworkConnectionCheckboxItem>
                    {
                new HeatNetworkConnectionCheckboxItem { IsSelected = true, Value = ConnectionType.CommunalBuildings.ToString(), ConditionalValue = 1 },
                new HeatNetworkConnectionCheckboxItem { IsSelected = true, Value = ConnectionType.IndividualHomes.ToString(), ConditionalValue = 2 },
                new HeatNetworkConnectionCheckboxItem { IsSelected = true, Value = ConnectionType.CommercialConnection.ToString(), ConditionalValue = 3 },
                new HeatNetworkConnectionCheckboxItem { IsSelected = true, Value = ConnectionType.OtherDistrictNetwork.ToString(), ConditionalValue = 4 }
                    }
                });
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.UserModel_Id_SessionKey)).Returns("user1");
            _sessionHelperMock.Setup(x => x.GetFromSession<string>(It.IsAny<HttpContext>(), SessionKeys.OrganisationId)).Returns("org1");
        }
    }
}