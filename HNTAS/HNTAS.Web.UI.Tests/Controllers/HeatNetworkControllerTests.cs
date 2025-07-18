using HNTAS.Web.UI.Controllers;
using HNTAS.Web.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HNTAS.Web.UI.Tests.Controllers
{
    public class HeatNetworkControllerTests
    {
        [Fact]
        public void EnterWhat3Words_Get_ReturnsViewResult_WithModel()
        {
            // Arrange
            var controller = new HeatNetworkController();

            // Act
            var result = controller.EnterWhat3Words() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.IsType<What3wordsUrlModel>(result.Model);
        }

        [Fact]
        public void EnterWhat3Words_Post_InvalidModel_ReturnsViewResult_WithModel()
        {
            // Arrange
            var controller = new HeatNetworkController();
            controller.ModelState.AddModelError("what3wordsUrl", "Required");
            var model = new What3wordsUrlModel();

            // Act
            var result = controller.EnterWhat3Words(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public void EnterWhat3Words_Post_ValidModel_RedirectsToNextStep()
        {
            // Arrange
            var controller = new HeatNetworkController();
            var model = new What3wordsUrlModel
            {
                what3wordsUrl = "index.home.raft"
            };

            // Act
            var result = controller.EnterWhat3Words(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            // Adjust the next step/action name as appropriate
            Assert.Equal("NextStepAction", redirect.ActionName);
        }
    }
}