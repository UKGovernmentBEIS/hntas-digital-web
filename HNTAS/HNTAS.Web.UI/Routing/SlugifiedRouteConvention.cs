using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HNTAS.Web.UI.Routing
{
    public class SlugifiedRouteConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var selector in controller.Selectors)
                {
                    // Apply [controller]/[action] if no route is already defined
                    if (selector.AttributeRouteModel == null)
                    {
                        selector.AttributeRouteModel = new AttributeRouteModel(
                            new RouteAttribute("[controller]/[action]"));
                    }
                }
            }
        }
    }
}
