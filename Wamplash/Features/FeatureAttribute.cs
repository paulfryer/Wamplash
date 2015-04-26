using System;

namespace Wamplash.Features
{
    public class FeatureAttribute : Attribute
    {
        public FeatureAttribute(string featureCode)
        {
            FeatureCode = featureCode;
        }

        public string FeatureCode { get; set; }
    }
}