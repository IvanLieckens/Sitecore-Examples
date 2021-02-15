using System;

using Sitecore.XConnect;

namespace Examples.EXM.Models.XConnect
{
    [Serializable]
    [FacetKey(DefaultFacetKey)]
    public class CustomFacet
    {
        public const string DefaultFacetKey = "CustomFacet";

        public string CustomValue { get; set; }
    }
}