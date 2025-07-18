using HNTAS.Web.UI.Extensions;
using Newtonsoft.Json;

namespace HNTAS.Web.UI.Helpers
{
    public static class SessionHelper
    {
        #region Constants

        public static class SessionKeys
        {
            public const string UserCreation_SessionKey = "UserModelDataKey";
            public const string OrganisationCreation_SessionKey = "OrganisationModelDataKey";

            // Session key for the boolean flow state
            public const string IsCheckAnswerFlowKey = "IsCheckAnswerFlow";
        }

        #endregion

        #region Generic Session Methods

        public static void SaveToSession<T>(HttpContext httpContext, string sessionKey, T model)
        {
            if (model == null)
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
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    // Optionally log the error
                    httpContext.Session.Remove(sessionKey);
                    return null;
                }
            }
            return null;
        }

        public static void ClearFromSession(HttpContext httpContext, string sessionKey)
        {
            httpContext.Session.Remove(sessionKey);
        }

        // You might still want a general ClearAllFlowRelatedSessionData if starting completely fresh
        public static void ClearAllFlowRelatedSessionData(HttpContext context)
        {
            ClearFromSession(context, SessionKeys.UserCreation_SessionKey);
            ClearFromSession(context, SessionKeys.OrganisationCreation_SessionKey);
            context.Session.Remove(SessionKeys.IsCheckAnswerFlowKey);
        }

        #endregion

        #region Flow State Methods

        public static void SetIsCheckAnswerFlow(HttpContext httpContext, bool isCheckAnswerFlow)
        {
            httpContext.Session.SetBoolean(SessionKeys.IsCheckAnswerFlowKey, isCheckAnswerFlow);
        }

        public static bool GetIsCheckAnswerFlow(HttpContext httpContext)
        {
            return httpContext.Session.GetBoolean(SessionKeys.IsCheckAnswerFlowKey) ?? false;
        }

        #endregion
    }
}