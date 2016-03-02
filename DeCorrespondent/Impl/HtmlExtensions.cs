using System;
using HtmlAgilityPack;

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
                return HtmlEntity.Entitize(s.Replace("​", string.Empty));
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
                return HtmlEntity.DeEntitize(s);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to de-entitize string '" + s + "'", e);
            }
        }
    }
}
