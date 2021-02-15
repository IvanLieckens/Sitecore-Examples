using System;

using Examples.EXM.Models.XConnect;

using Sitecore.Data;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.Core.Pipelines.GetContact;

namespace Examples.EXM.Pipelines.GetContact
{
    public class GetExtendedContact
    {
        private readonly IContactService _contactService;

        public GetExtendedContact(IContactService contactService)
        {
            _contactService = contactService;
        }

        public void Process(GetContactPipelineArgs args)
        {
            string[] facetKeys = args.FacetKeys;
            Array.Resize(ref facetKeys, facetKeys.Length + 1);
            facetKeys[facetKeys.Length - 1] = CustomFacet.DefaultFacetKey;

            if (args.ContactIdentifier == null && ID.IsNullOrEmpty(args.ContactId))
            {
                throw new ArgumentException("Either the contact identifier or the contact id must be set");
            }

            args.Contact = args.ContactIdentifier != null ? _contactService.GetContact(args.ContactIdentifier, facetKeys) : _contactService.GetContact(args.ContactId, facetKeys);
        }
    }
}