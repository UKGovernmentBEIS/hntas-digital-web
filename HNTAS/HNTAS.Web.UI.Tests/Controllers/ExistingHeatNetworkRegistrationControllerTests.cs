using HNTAS.Api.Client.Model;
using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Helpers;
using HNTAS.Web.UI.Models;
using HNTAS.Web.UI.Models.HeatNetworkRegistration;
using HNTAS.Web.UI.Services;
using HNTAS.Web.UI.Services.Core;
using HNTAS.Web.UI.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Moq;
using System.Net.Http;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class ExistingHeatNetworkRegistrationControllerTests
    {
        private readonly Mock<ISessionHelper> _sessionHelperMock;
        private readonly Mock<IHeatNetworkService> _heatNetworkServiceMock;
        private readonly Mock<IOrganisationService> _organisationServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<ExistingHeatNetworkRegistrationController>> _loggerMock;
        private readonly ExistingHeatNetworkRegistrationController _controller;

        public ExistingHeatNetworkRegistrationControllerTests()
        {
            _sessionHelperMock = new Mock<ISessionHelper>();
            _heatNetworkServiceMock = new Mock<IHeatNetworkService>();
            _organisationServiceMock = new Mock<IOrganisationService>();
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<ExistingHeatNetworkRegistrationController>>();
            _controller = CreateController();
        }

        private Mock<IUrlHelper> SetUpBackLink(string action)
        {
            return TestingUtility.SetUpBackLink("HeatNetworkRegistration", action);
        }

        private ExistingHeatNetworkRegistrationController CreateController()
        {
            var controller = new ExistingHeatNetworkRegistrationController(
                _sessionHelperMock.Object,
                _heatNetworkServiceMock.Object,
                _organisationServiceMock.Object,
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
            var urlHelperMock = new Mock<IUrlHelper>();
            controller.Url = urlHelperMock.Object;
            return controller;

        }

        [Fact]
        public async Task HeatNetworkDwellingsCheck_ShouldReturnViewAndStoreSessionValues()
        {
            // Arrange
            var hnId = "HN1000001";

            var heatNetwork = new HeatNetworkResponse
            {
                HnId = hnId,
                Name = "test1",
                Address = new RegisteredAddress(
                    addressLine1: string.Empty,
                    postcode: string.Empty,
                    addressLine2: default,
                    town: default,
                    county: default,
                    country: default
                ),
                EcDetails = new ECDetails(latitude: 1.1, longitude: 1.1)
            };

            _heatNetworkServiceMock
                .Setup(x => x.GetAsync(hnId))
                .ReturnsAsync(heatNetwork);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(new HowManyDwellingsIncludedModel());

            // Act
            var result = await _controller.HeatNetworkDwellingsCheck(hnId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HowManyDwellingsIncludedModel>(viewResult.Model);

            Assert.Equal(hnId, viewResult.ViewData["HnId"]);

            _heatNetworkServiceMock.Verify(x => x.GetAsync(hnId), Times.Once);

            _sessionHelperMock.Verify(x => x.SaveToSession(
                It.IsAny<HttpContext>(),
                SessionKeys.HnId,
                hnId), Times.Once);
        }

        [Fact]
        public async Task HeatNetworkDwellingsCheck_ShouldCreateNewModel_WhenSessionModelIsNull()
        {
            // Arrange
            var hnId = "HN1000001";

            _heatNetworkServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HeatNetworkResponse
                {
                    HnId = hnId,
                    Name = "test1",
                    Address = new RegisteredAddress(
                    addressLine1: string.Empty,
                    postcode: string.Empty,
                    addressLine2: default,
                    town: default,
                    county: default,
                    country: default
                    ),
                    EcDetails = new ECDetails(latitude: 1.1, longitude: 1.1)
                });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns((HowManyDwellingsIncludedModel)null);

            // Act
            var result = await _controller.HeatNetworkDwellingsCheck(hnId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<HowManyDwellingsIncludedModel>(viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel();

            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_ShouldRedirectToHeatNetworkIntroduction_WhenAnswerIsYes()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel
            {
                HowManyDwellingsIncluded = "yes"
            };

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkIntroduction", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkDwellingsCheck_Post_ShouldRedirectToSixOrMoreDwellingsAnswerNo_WhenAnswerIsUnexpected()
        {
            // Arrange
            var model = new HowManyDwellingsIncludedModel
            {
                HowManyDwellingsIncluded = "anything"
            };

            // Act
            var result = _controller.HeatNetworkDwellingsCheck(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("SixOrMoreDwellingsAnswerNo", redirectResult.ActionName);
        }
                
        [Fact]
        public void SixOrMoreDwellingsAnswerNo_ShouldReturnView()
        {
            // Act
            var result = _controller.SixOrMoreDwellingsAnswerNo();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);            
        }

        [Fact]
        public void HeatNetworkIntroduction_ShouldReturnView()
        {
            // Arrange
            var hnId = "HN1000001";

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            // Act
            var result = _controller.HeatNetworkIntroduction();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkType_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = true
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkType();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkType_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel();

            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkType_Post_ShouldRedirectToHeatNetworkEcCommunal_WhenCommunal()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = true
            };

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkEcCommunal", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.IsHnTypeCommunalViewModel,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkType_Post_ShouldRedirectToHeatNetworkEcDistrict_WhenDistrict()
        {
            // Arrange
            var model = new IsHnTypeCommunalViewModel
            {
                IsHnTypeCommunal = false
            };

            // Act
            var result = _controller.HeatNetworkType(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkEcDistrict", redirectResult.ActionName);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkEcCommunal();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel();

            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_ShouldRedirectToHeatNetworkCommunalOneBlock_WhenHasOwnEc()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkCommunalOneBlock", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalHnHaveOwnEcViewModel,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkEcCommunal_Post_ShouldRedirectToHeatNetworkCommunalNoECSummary_WhenHasNoOwnEc()
        {
            // Arrange
            var model = new DoesCommunalHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            // Act
            var result = _controller.HeatNetworkEcCommunal(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkCommunalNoECSummary", redirectResult.ActionName);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = true
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesCommunalEcSupplyOneBlockViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalEcSupplyOneBlockViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel();

            _controller.ModelState.AddModelError("test", "error");

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_ShouldRedirectToHeatNetworkCommunalECSummary_WhenSuppliesOneBlock()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = true
            };

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkCommunalECSummary", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalEcSupplyOneBlockViewModel,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlock_Post_ShouldRedirectToHeatNetworkCommunalOneBlockSummary_WhenDoesNotSupplyOneBlock()
        {
            // Arrange
            var model = new DoesCommunalEcSupplyOneBlockViewModel
            {
                SuppliesOneBlock = false
            };

            // Act
            var result = _controller.HeatNetworkCommunalOneBlock(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkCommunalOneBlockSummary", redirectResult.ActionName);
        }

        [Fact]
        public void HeatNetworkCommunalECSummary_ShouldReturnView()
        {
            // Arrange
            var hnId = "HN1000001";

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            // Act
            var result = _controller.HeatNetworkCommunalECSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(hnId, _controller.ViewBag.HnId);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName",
                    "HeatNetworkCommunalECSummary"),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalOneBlockSummary_ShouldReturnView()
        {
            // Arrange
            var hnId = "HN1000001";

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            // Act
            var result = _controller.HeatNetworkCommunalOneBlockSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(hnId, _controller.ViewBag.HnId);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName",
                    "HeatNetworkCommunalOneBlockSummary"),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkCommunalNoECSummary_ShouldReturnView()
        {
            // Arrange
            var hnId = "HN1000001";

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            // Act
            var result = _controller.HeatNetworkCommunalNoECSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(hnId, _controller.ViewBag.HnId);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName",
                    "HeatNetworkCommunalNoECSummary"),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkEcDistrict();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("HeatNetworkRegistration/HeatNetworkEcDistrict", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel();

            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_ShouldRedirectToHeatNetworkConnections_WhenHasOwnEc()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = true
            };

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkConnections", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkEcDistrict_Post_ShouldSaveConnectionsModelAndRedirect_WhenHasNoOwnEc()
        {
            // Arrange
            var model = new DoesDistrictHnHaveOwnEcViewModel
            {
                HasOwnEc = false
            };

            // Act
            var result = _controller.HeatNetworkEcDistrict(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkConnections", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel,
                    model),
                Times.Once);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey,
                    It.IsAny<HeatNetworkConnectionsViewModel>()),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkConnections_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>()
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkConnections();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkConnections_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
        {
            new()
        }
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            _controller.ModelState.AddModelError("Test", "Error");

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public void HeatNetworkConnections_Post_ShouldRedirectToHeatNetworkDistrictEcSummary_WhenHasOwnEc()
        {
            // Arrange
            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new()
                }
            };

            var original = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new()
                    {
                        Label = "Label",
                        HintText = "Hint",
                        Value = "Value"
                    }
                }
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(original);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel
                {
                    HasOwnEc = true
                });

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkDistrictEcSummary", redirectResult.ActionName);
        }

        [Fact]
        public void HeatNetworkConnections_Post_ShouldRedirectToHeatNetworkDistrictNoEcSummary_WhenHasNoOwnEc()
        {
            // Arrange
            var model = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new()
                }
            };

            var original = new HeatNetworkConnectionsViewModel
            {
                Connections = new List<HeatNetworkConnectionCheckboxItem>
                {
                    new()
                    {
                        Label = "Label",
                        HintText = "Hint",
                        Value = "Value"
                    }
                }
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(original);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesDistrictHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesDistrictHnHaveOwnEcViewModel))
                .Returns(new DoesDistrictHnHaveOwnEcViewModel
                {
                    HasOwnEc = false
                });

            // Act
            var result = _controller.HeatNetworkConnections(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkDistrictNoEcSummary", redirectResult.ActionName);
        }

        [Fact]
        public void HeatNetworkDistrictEcSummary_ShouldReturnViewWithModel()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new HeatNetworkConnectionsViewModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkDistrictEcSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName",
                    "HeatNetworkDistrictEcSummary"),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkDistrictNoEcSummary_ShouldReturnViewWithModel()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new HeatNetworkConnectionsViewModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkConnectionsViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkConnectionsViewModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkDistrictNoEcSummary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName",
                    "HeatNetworkDistrictNoEcSummary"),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkName_Get_ShouldReturnViewWithModelFromSession()
        {
            // Arrange
            var hnId = "HN1000001";

            var model = new HeatNetworkNameModel
            {
                HeatNetworkName = "Test Network"
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName"))
                .Returns("HeatNetworkDistrictEcSummary");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkNameModelKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkName();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.Equal(hnId, _controller.ViewBag.HnId);
        }

        [Fact]
        public void HeatNetworkName_Post_ShouldRedirectToCheckYourAnswers()
        {
            // Arrange
            var model = new HeatNetworkNameModel
            {
                HeatNetworkName = "Test Network"
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName"))
                .Returns("HeatNetworkDistrictEcSummary");

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("CheckYourAnswers", redirectResult.ActionName);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkNameModelKey,
                    model),
                Times.Once);
        }

        [Fact]
        public void HeatNetworkName_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new HeatNetworkNameModel
            {
                HeatNetworkName = string.Empty
            };

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    "backActionFromHnName"))
                .Returns("HeatNetworkDistrictEcSummary");

            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.HeatNetworkName(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<HeatNetworkNameModel>()),
                Times.Never);
        }

        [Fact]
        public async Task CheckYourAnswersAsync_ShouldRedirectToUserAccount_WhenRequiredSessionDataMissing()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkNameModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkNameModelKey))
                .Returns((HeatNetworkNameModel)null);

            // Act
            var result = await _controller.CheckYourAnswersAsync();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("UserAccount", redirectResult.ActionName);
            Assert.Equal("Dashboard", redirectResult.ControllerName);
        }

        [Fact]
        public async Task CheckYourAnswersAsync_ShouldReturnView_WhenRequiredDataExists()
        {
            // Arrange
            _sessionHelperMock.Setup(x =>
                x.GetFromSession<HowManyDwellingsIncludedModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HowManyDwellingsIncludedModelKey))
                .Returns(new HowManyDwellingsIncludedModel
                {
                    HowManyDwellingsIncluded = "yes"
                });

            _sessionHelperMock.Setup(x =>
                x.GetFromSession<HeatNetworkOrganisationModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkOrganisationModelKey))
                .Returns(new HeatNetworkOrganisationModel());

            _sessionHelperMock.Setup(x =>
                x.GetFromSession<HeatNetworkNameModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkNameModelKey))
                .Returns(new HeatNetworkNameModel { HeatNetworkName = "Test HN" });

            _sessionHelperMock.Setup(x =>
                x.GetFromSession<HeatNetworkPhaseModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkPhaseModelKey))
                .Returns(new HeatNetworkPhaseModel());

            _sessionHelperMock.Setup(x =>
                x.GetFromSession<IsHnTypeCommunalViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel
                {
                    IsHnTypeCommunal = true
                });

            // Act
            var result = await _controller.CheckYourAnswersAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<CheckYourAnswersHeatNetworkModel>(viewResult.Model);

            _sessionHelperMock.Verify(x =>
                x.SaveToSession(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CheckYourAnswersHeatNetworkModelKey,
                    It.IsAny<CheckYourAnswersHeatNetworkModel>()),
                Times.Once);
        }

        [Fact]
        public async Task SubmitAnswers_ShouldReturnCheckYourAnswersView_WhenDeclarationNotConfirmed()
        {
            // Arrange
            var viewModel = new CheckYourAnswersHeatNetworkModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(viewModel);

            // Act
            var result = await _controller.SubmitAnswers(false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Equal(viewModel, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task SubmitAnswers_ShouldReturnCheckYourAnswersView_WhenHnIdIsNull()
        {
            // Arrange
            var viewModel = new CheckYourAnswersHeatNetworkModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(viewModel);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns((string)null);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel());

            // Act
            var result = await _controller.SubmitAnswers(true);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("HeatNetworkRegistration/CheckYourAnswers", viewResult.ViewName);
            Assert.Equal(viewModel, viewResult.Model);
        }

        [Fact]
        public async Task SubmitAnswers_ShouldRedirectToCompletion_WhenRegistrationSucceeds()
        {
            // Arrange
            var hnId = "HN1000001";

            var cyaModel = new CheckYourAnswersHeatNetworkModel();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<CheckYourAnswersHeatNetworkModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.CheckYourAnswersHeatNetworkModelKey))
                .Returns(cyaModel);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns(hnId);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<IsHnTypeCommunalViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.IsHnTypeCommunalViewModel))
                .Returns(new IsHnTypeCommunalViewModel
                {
                    IsHnTypeCommunal = true
                });

            _sessionHelperMock
                .Setup(x => x.GetFromSession<DoesCommunalHnHaveOwnEcViewModel>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.DoesCommunalHnHaveOwnEcViewModel))
                .Returns(new DoesCommunalHnHaveOwnEcViewModel
                {
                    HasOwnEc = true
                });

            _heatNetworkServiceMock
                .Setup(x => x.GetAsync(hnId))
                .ReturnsAsync(new HeatNetworkResponse
                {                    
                    HnId = hnId
                });

            _heatNetworkServiceMock
                .Setup(x => x.RegisterOfgemNetwork(It.IsAny<HeatNetwork>()))
                .ReturnsAsync(new HeatNetworkResponse
                {
                    HnId = hnId,
                    Name = "Test Network"
                });

            // Act
            var result = await _controller.SubmitAnswers(true);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("HeatNetworkRegistrationComplete", redirectResult.ActionName);
        }

        [Fact]
        public async Task HeatNetworkRegistrationComplete_ShouldReturnViewWithConfirmationDetails()
        {
            // Arrange
            _controller.TempData["Confirmation_HN_Id"] = "HN1000001";
            _controller.TempData["HNName"] = "Test Network";
            _controller.TempData["AdditionalDescription"] = "Block A";

            // Act
            var result = await _controller.HeatNetworkRegistrationComplete();

            // Assert
            Assert.IsType<ViewResult>(result);

            Assert.Equal("HN1000001", _controller.ViewBag.HnId);
            Assert.Equal("Test Network", _controller.ViewBag.HNName);
            Assert.Equal("Test Network, Block A", _controller.ViewBag.HNNameWithDescription);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_ShouldReturnViewWithModel()
        {
            // Arrange
            var model = new HeatNetworkSuccessRedirection();

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns("HN1000001");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnName))
                .Returns("Test Network");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<RegistrationSource>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.RegistrationSourceKey))
                .Returns(RegistrationSource.OFGEM);

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkSuccessRedirection>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkSuccessRedirectionSessionKey))
                .Returns(model);

            // Act
            var result = _controller.HeatNetworkSuccessRedirection();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Equal(model, viewResult.Model);
            Assert.Equal("HN1000001", _controller.ViewBag.HnId);
            Assert.Equal("Test Network", _controller.ViewBag.HNName);
        }

        [Fact]
        public void HeatNetworkSuccessRedirection_ShouldCreateNewModel_WhenSessionModelIsNull()
        {
            // Arrange
            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnId))
                .Returns("HN1000001");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<string>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HnName))
                .Returns("Test Network");

            _sessionHelperMock
                .Setup(x => x.GetFromSession<HeatNetworkSuccessRedirection>(
                    It.IsAny<HttpContext>(),
                    SessionKeys.HeatNetworkSuccessRedirectionSessionKey))
                .Returns((HeatNetworkSuccessRedirection)null);

            // Act
            var result = _controller.HeatNetworkSuccessRedirection();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(viewResult.Model);
            Assert.IsType<HeatNetworkSuccessRedirection>(viewResult.Model);

            Assert.Equal("HN1000001", _controller.ViewBag.HnId);
            Assert.Equal("Test Network", _controller.ViewBag.HNName);
        }
    }
}
