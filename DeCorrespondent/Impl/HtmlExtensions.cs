using System;
using System.Web;

namespace DeCorrespondent.Impl
{
    public static class HtmlExtensions
    {
        public static string EscapeHtml(this string s)
        {
            if (s == null)
                return null;
            try
            {
                return HttpUtility.HtmlEncode(s);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to entitize string '" + s + "'", e);
            }
        }

        public static string UnescapeHtml(this string s)
        {
            try
            {
                return HttpUtility.HtmlDecode(s);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to de-entitize string '" + s + "'", e);
            }
        }
    }
}
