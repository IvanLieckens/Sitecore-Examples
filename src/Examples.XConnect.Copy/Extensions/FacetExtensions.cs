using Sitecore.XConnect;
using Sitecore.XConnect.Serialization;

namespace Examples.XConnect.Copy.Extensions
{
    public static class FacetExtensions
    {
        public static T WithClearedConcurrency<T>(this T facet) where T : Facet
        {
            DeserializationHelpers.SetConcurrencyToken(facet, null);
            return facet;
        }
    }
}
