using System.Text.RegularExpressions;

namespace HNTAS.Web.UI.Routing
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null) { return null; }

            // Convert the value to kebab-case
            // Example: "ListAll" -> "list-all"
            return Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
        }
    }
}