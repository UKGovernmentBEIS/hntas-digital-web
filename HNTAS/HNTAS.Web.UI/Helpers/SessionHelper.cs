using HNTAS.Web.UI.Models;
using Newtonsoft.Json;

namespace HNTAS.Web.UI.Helpers
{
    public static class SessionHelper
    {

        public static void SaveToSession<T>(HttpContext httpContext, string sessionKey, T model)
        {
            if (model == null) // Handle null model gracefully
            {
                httpContext.Session.Remove(sessionKey);
                return;
            }
            string json = JsonConvert.SerializeObject(model);
            httpContext.Session.SetString(sessionKey, json);
        }

        public static T? GetFromSession<T>(HttpContext httpContext, string sessionKey) where T : class
        {
            string? json = httpContext.Session.GetString(sessionKey);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            return null;
        }

        public static void RemoveFromSession(HttpContext httpContext, string sessionKey)
        {
            httpContext.Session.Remove(sessionKey);
        }

    }
}
