using Sitecore.Data;

// ReSharper disable StyleCop.SA1401 - Using public static/const fields for centralized reference.
namespace Examples.Wildcard
{
    public static class Templates
    {
        public static class Person
        {
            public static ID TemplateId = new ID("{D636C41B-DDD6-4CEC-8F2B-A2E15A449B38}");

            public static class Fields
            {
                public static ID PersonFirstName = new ID("{376F836C-A106-4C42-997A-372044F551ED}");

                public static ID PersonLastName = new ID("{20B06DDE-4204-4544-95BE-A3F855A5F055}");

                public static ID PersonEmail = new ID("{31CCCFFD-8D3F-4F02-B4B1-DC84D53F46A2}");

                public static ID PersonPhone = new ID("{4B4C602E-7983-4C4A-9A74-442F04922C08}");
            }
        }
    }
}