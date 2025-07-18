namespace HNTAS.Web.UI.Extensions
{
    public static class SessionExtensions
    {
        public static void SetBoolean(this ISession session, string key, bool value)
        {
            session.SetString(key, value.ToString());
        }

        public static bool? GetBoolean(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (bool.TryParse(value, out var result))
                return result;
            return null;
        }
    }
}
