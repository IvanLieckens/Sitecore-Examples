using System;
using System.Collections.Generic;
using System.Linq;

using Examples.XConnect.Copy.Config;
using Examples.XConnect.Copy.Extensions;
using Examples.XConnect.Copy.Status;

using Serilog;

using Sitecore.ContentTesting.Model.xConnect;
using Sitecore.EmailCampaign.Model.XConnect.Events;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;

namespace Examples.XConnect.Copy.Write
{
    public class XConnectWriter : Writer
    {
        private readonly XConnectClient _client;

        private readonly Configuration _config;

        public XConnectWriter(XConnectClient client, Configuration config)
        {
            _client = client;
            _config = config;
        }

        protected override void ProcessContact(Contact contact)
        {
            Log.Logger.Debug($"Started processing contact {contact.Id}");
            ContactIdentifier identifier =
                contact.Identifiers.FirstOrDefault(i => i.Source == _config.IdentifierSource);
            if (identifier != null || contact.Interactions.Any(i => i.EndDateTime > DateTime.UtcNow.AddYears(-1)))
            {
                Contact existingContact = SearchContact(contact);
                if (existingContact != null)
                {
                    Log.Logger.Debug($"Found a contact for {contact.Id}, updating");
                    UpdateContact(existingContact, contact);
                }
                else
                {
                    Log.Logger.Debug($"Contact {contact.Id} is new, adding");
                    AddContact(contact);
                }
            }
            else
            {
                Log.Logger.Information($"Contact {contact.Id} was skipped");
                CurrentStatus.SkippedCounterAdd(1);
            }
        }

        private static void CopyEvents(Interaction source, Interaction target)
        {
            foreach (Event e in source.Events)
            {
                Event result;
                if (e is CampaignEvent ce)
                {
                    CampaignEvent newEvent = new CampaignEvent(ce.CampaignDefinitionId, ce.Timestamp);
                    result = newEvent;
                }
                else if (e is DownloadEvent de)
                {
                    DownloadEvent newEvent = new DownloadEvent(de.Timestamp, de.ItemId);
                    result = newEvent;
                }
                else if (e is Goal g)
                {
                    Goal newEvent = new Goal(g.DefinitionId, g.Timestamp);
                    result = newEvent;
                }
                else if (e is MVTestTriggered mvt)
                {
                    MVTestTriggered newEvent = new MVTestTriggered(mvt.Timestamp);
                    newEvent.Combination = mvt.Combination;
                    newEvent.EligibleRules = mvt.EligibleRules;
                    newEvent.ExposureTime = mvt.ExposureTime;
                    newEvent.FirstExposure = mvt.FirstExposure;
                    newEvent.IsSuspended = mvt.IsSuspended;
                    newEvent.ValueAtExposure = mvt.ValueAtExposure;
                    result = newEvent;
                }
                else if (e is Outcome o)
                {
                    Outcome newEvent = new Outcome(o.DefinitionId, o.Timestamp, o.CurrencyCode, o.MonetaryValue);
                    result = newEvent;
                }
                else if (e is PageViewEvent pve)
                {
                    PageViewEvent newEvent = new PageViewEvent(
                        pve.Timestamp,
                        pve.ItemId,
                        pve.ItemVersion,
                        pve.ItemLanguage);
                    newEvent.SitecoreRenderingDevice = pve.SitecoreRenderingDevice;
                    newEvent.Url = pve.Url;
                    result = newEvent;
                }
                else if (e is PersonalizationEvent pe)
                {
                    PersonalizationEvent newEvent = new PersonalizationEvent(pe.Timestamp);
                    newEvent.ExposedRules = pe.ExposedRules;
                    result = newEvent;
                }
                else if (e is SearchEvent se)
                {
                    SearchEvent newEvent = new SearchEvent(se.Timestamp);
                    newEvent.Keywords = se.Keywords;
                    result = newEvent;
                }
                else if (e is BounceEvent be)
                {
                    BounceEvent newEvent = new BounceEvent(be.Timestamp);
                    newEvent.BounceReason = be.BounceReason;
                    newEvent.BounceType = be.BounceType;
                    result = newEvent;
                }
                else if (e is DispatchFailedEvent dfe)
                {
                    DispatchFailedEvent newEvent = new DispatchFailedEvent(dfe.Timestamp);
                    newEvent.FailureReason = dfe.FailureReason;
                    result = newEvent;
                }
                else if (e is EmailClickedEvent ece)
                {
                    EmailClickedEvent newEvent = new EmailClickedEvent(ece.Timestamp);
                    newEvent.Url = ece.Url;
                    result = newEvent;
                }
                else if (e is EmailOpenedEvent eoe)
                {
                    EmailOpenedEvent newEvent = new EmailOpenedEvent(eoe.Timestamp);
                    result = newEvent;
                }
                else if (e is EmailSentEvent ese)
                {
                    EmailSentEvent newEvent = new EmailSentEvent(ese.Timestamp);
                    result = newEvent;
                }
                else if (e is SpamComplaintEvent sce)
                {
                    SpamComplaintEvent newEvent = new SpamComplaintEvent(sce.Timestamp);
                    result = newEvent;
                }
                else if (e is UnsubscribedFromEmailEvent uee)
                {
                    UnsubscribedFromEmailEvent newEvent = new UnsubscribedFromEmailEvent(uee.Timestamp);
                    result = newEvent;
                }
                else
                {
                    result = new Event(e.DefinitionId, e.Timestamp);
                }

                // Many of the above are derived from EmailEvent, so only copy relevant properties once
                if (e is EmailEvent ee && result is EmailEvent nee)
                {
                    nee.EmailAddressHistoryEntryId = ee.EmailAddressHistoryEntryId;
                    nee.InstanceId = ee.InstanceId;
                    nee.ManagerRootId = ee.ManagerRootId;
                    nee.MessageId = ee.MessageId;
                    nee.MessageLanguage = ee.MessageLanguage;
                    nee.TestValueIndex = ee.TestValueIndex;
                }

                result.Data = e.Data;
                result.DataKey = e.DataKey;
                result.Duration = e.Duration;
                result.EngagementValue = e.EngagementValue;
                result.Id = e.Id;
                result.ParentEventId = e.ParentEventId;
                result.Text = e.Text;
                foreach (KeyValuePair<string, string> customValue in e.CustomValues)
                {
                    result.CustomValues.Add(customValue.Key, customValue.Value);
                }

                if (result != null)
                {
                    target.Events.Add(result);
                }
            }
        }

        private static IEnumerable<Interaction> FindMissingInteractions(Contact target, Contact source)
        {
            DateTime? lastModified = target.Interactions.Max(i => i.LastModified);
            return lastModified.HasValue
                       ? source.Interactions.Where(i => i.LastModified > lastModified.Value)
                       : new Interaction[0];
        }

        private void UpdateContact(Contact target, Contact source)
        {
            if (source.LastModified > target.LastModified)
            {
                Log.Logger.Debug($"Contact {target.Id} was updated from {source.Id}'s facets");
                SetContact(target, source);
            }

            if (source.Interactions.Count > target.Interactions.Count)
            {
                foreach (Interaction interaction in FindMissingInteractions(target, source))
                {
                    Log.Logger.Debug($"Added missing interaction {interaction.Id} to {target.Id} from {source.Id}");
                    CopyInteraction(interaction, target);
                }
            }

            CurrentStatus.UpdateCounterAdd(1);
        }

        private void SetContact(Contact target, Contact source)
        {
            // Default Facets
            if (source.Facets.ContainsKey(PersonalInformation.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(PersonalInformation.DefaultFacetKey))
                {
                    PersonalInformation sourceFacet = source.GetFacet<PersonalInformation>();
                    PersonalInformation targetFacet = target.GetFacet<PersonalInformation>();
                    targetFacet.Birthdate = sourceFacet.Birthdate;
                    targetFacet.FirstName = sourceFacet.FirstName;
                    targetFacet.Gender = sourceFacet.Gender;
                    targetFacet.JobTitle = sourceFacet.JobTitle;
                    targetFacet.Title = sourceFacet.Title;
                    targetFacet.LastName = sourceFacet.LastName;
                    targetFacet.MiddleName = sourceFacet.MiddleName;
                    targetFacet.Nickname = sourceFacet.Nickname;
                    targetFacet.PreferredLanguage = sourceFacet.PreferredLanguage;
                    targetFacet.Suffix = sourceFacet.Suffix;
                    _client.SetFacet(target, PersonalInformation.DefaultFacetKey, targetFacet);
                }
                else
                {
                    _client.SetFacet(
                        target,
                        PersonalInformation.DefaultFacetKey,
                        source.GetFacet<PersonalInformation>().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(EmailAddressList.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(EmailAddressList.DefaultFacetKey))
                {
                    EmailAddressList sourceFacet = source.GetFacet<EmailAddressList>();
                    EmailAddressList targetFacet = target.GetFacet<EmailAddressList>();
                    targetFacet.Others = sourceFacet.Others;
                    targetFacet.PreferredEmail = sourceFacet.PreferredEmail;
                    targetFacet.PreferredKey = sourceFacet.PreferredKey;
                    _client.SetFacet(target, EmailAddressList.DefaultFacetKey, targetFacet);
                }
                else
                {
                    _client.SetFacet(
                        target,
                        EmailAddressList.DefaultFacetKey,
                        source.GetFacet<EmailAddressList>().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(AddressList.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(AddressList.DefaultFacetKey))
                {
                    AddressList sourceFacet = source.Addresses();
                    AddressList targetFacet = target.Addresses();
                    targetFacet.PreferredKey = sourceFacet.PreferredKey;
                    targetFacet.Others = sourceFacet.Others;
                    targetFacet.PreferredAddress = sourceFacet.PreferredAddress;
                    _client.SetAddresses(target, targetFacet);
                }
                else
                {
                    _client.SetAddresses(target, source.Addresses().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(PhoneNumberList.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(PhoneNumberList.DefaultFacetKey))
                {
                    PhoneNumberList sourceFacet = source.PhoneNumbers();
                    PhoneNumberList targetFacet = target.PhoneNumbers();
                    targetFacet.PreferredKey = sourceFacet.PreferredKey;
                    targetFacet.Others = sourceFacet.Others;
                    targetFacet.PreferredPhoneNumber = sourceFacet.PreferredPhoneNumber;
                    _client.SetPhoneNumbers(target, targetFacet);
                }
                else
                {
                    _client.SetPhoneNumbers(target, source.PhoneNumbers().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(ConsentInformation.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(ConsentInformation.DefaultFacetKey))
                {
                    ConsentInformation sourceFacet = source.ConsentInformation();
                    ConsentInformation targetFacet = target.ConsentInformation();
                    targetFacet.ConsentRevoked = sourceFacet.ConsentRevoked;
                    targetFacet.DoNotMarket = sourceFacet.DoNotMarket;
                    targetFacet.ExecutedRightToBeForgotten = sourceFacet.ExecutedRightToBeForgotten;
                    _client.SetConsentInformation(target, targetFacet);
                }
                else
                {
                    _client.SetConsentInformation(target, source.ConsentInformation().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(ListSubscriptions.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(ListSubscriptions.DefaultFacetKey))
                {
                    ListSubscriptions sourceFacet = source.ListSubscriptions();
                    ListSubscriptions targetFacet = target.ListSubscriptions();
                    targetFacet.Subscriptions = sourceFacet.Subscriptions;
                    _client.SetListSubscriptions(target, targetFacet);
                }
                else
                {
                    _client.SetListSubscriptions(target, source.ListSubscriptions().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(Avatar.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(Avatar.DefaultFacetKey))
                {
                    Avatar sourceFacet = source.Avatar();
                    Avatar targetFacet = target.Avatar();
                    targetFacet.MimeType = sourceFacet.MimeType;
                    targetFacet.Picture = sourceFacet.Picture;
                    _client.SetAvatar(target, targetFacet);
                }
                else
                {
                    _client.SetAvatar(target, source.Avatar().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey(EmailAddressHistory.DefaultFacetKey))
            {
                if (target.Facets.ContainsKey(EmailAddressHistory.DefaultFacetKey))
                {
                    EmailAddressHistory sourceFacet = source.GetFacet<EmailAddressHistory>();
                    EmailAddressHistory targetFacet = target.GetFacet<EmailAddressHistory>();
                    targetFacet.History = sourceFacet.History;
                    _client.SetFacet(target, EmailAddressHistory.DefaultFacetKey, targetFacet);
                }
                else
                {
                    _client.SetFacet(
                        target,
                        EmailAddressHistory.DefaultFacetKey,
                        source.GetFacet<EmailAddressHistory>().WithClearedConcurrency());
                }
            }

            if (source.Facets.ContainsKey("TestCombinations"))
            {
                if (target.Facets.ContainsKey("TestCombinations"))
                {
                    TestCombinationsData sourceFacet = source.Facets["TestCombinations"] as TestCombinationsData;
                    if (target.Facets["TestCombinations"] is TestCombinationsData targetFacet)
                    {
                        targetFacet.TestCombinations = sourceFacet?.TestCombinations;
                        _client.SetFacet(target, "TestCombinations", targetFacet);
                    }
                }
                else
                {
                    Facet oldFacet = source.Facets["TestCombinations"];
                    TestCombinationsData newFacet = new TestCombinationsData();
                    newFacet.TestCombinations = oldFacet.XObject["TestCombinations"]?.ToString();
                    _client.SetFacet(target, "TestCombinations", newFacet);
                }
            }

            // TODO Handle custom facets

            _client.Submit();
        }

        private void AddContact(Contact contact)
        {
            Contact newContact = CopyContact(contact);
            foreach (Interaction interaction in contact.Interactions)
            {
                CopyInteraction(interaction, newContact);
            }

            CurrentStatus.AddCounterAdd(1);
        }

        private Contact CopyContact(Contact contact)
        {
            List<ContactIdentifier> identifiers = contact.Identifiers.Select(
                identifier => identifier.Source != "Alias"
                                  ? identifier
                                  : new ContactIdentifier(
                                      "AliasOld",
                                      identifier.Identifier,
                                      ContactIdentifierType.Anonymous)).ToList();
            Contact result = new Contact(identifiers.ToArray());
            _client.AddContact(result);

            // Default Facets
            if (contact.Facets.ContainsKey(PersonalInformation.DefaultFacetKey))
            {
                _client.SetFacet(
                    result,
                    PersonalInformation.DefaultFacetKey,
                    contact.GetFacet<PersonalInformation>().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(EmailAddressList.DefaultFacetKey))
            {
                _client.SetFacet(
                    result,
                    EmailAddressList.DefaultFacetKey,
                    contact.GetFacet<EmailAddressList>().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(AddressList.DefaultFacetKey))
            {
                _client.SetAddresses(result, contact.Addresses().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(PhoneNumberList.DefaultFacetKey))
            {
                _client.SetPhoneNumbers(result, contact.PhoneNumbers().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(ConsentInformation.DefaultFacetKey))
            {
                _client.SetConsentInformation(result, contact.ConsentInformation().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(ListSubscriptions.DefaultFacetKey))
            {
                _client.SetListSubscriptions(result, contact.ListSubscriptions().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(Avatar.DefaultFacetKey))
            {
                _client.SetAvatar(result, contact.Avatar().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey(EmailAddressHistory.DefaultFacetKey))
            {
                _client.SetFacet(
                    result,
                    EmailAddressHistory.DefaultFacetKey,
                    contact.GetFacet<EmailAddressHistory>().WithClearedConcurrency());
            }

            if (contact.Facets.ContainsKey("TestCombinations"))
            {
                Facet oldFacet = contact.Facets["TestCombinations"];
                TestCombinationsData newFacet = new TestCombinationsData();
                newFacet.TestCombinations = oldFacet.XObject["TestCombinations"]?.ToString();
                _client.SetFacet(result, "TestCombinations", newFacet);
            }

            // TODO Handle custom facets

            _client.Submit();

            return result;
        }

        private void CopyInteraction(Interaction interaction, Contact contact)
        {
            Interaction result = new Interaction(
                contact,
                interaction.Initiator,
                interaction.ChannelId,
                interaction.UserAgent);
            CopyEvents(interaction, result);
            _client.AddInteraction(result);

            if (interaction.Facets.ContainsKey(UserAgentInfo.DefaultFacetKey))
            {
                _client.SetFacet(
                    result,
                    UserAgentInfo.DefaultFacetKey,
                    interaction.UserAgentInfo().WithClearedConcurrency());
            }

            if (interaction.Facets.ContainsKey(IpInfo.DefaultFacetKey))
            {
                _client.SetFacet(result, IpInfo.DefaultFacetKey, interaction.IpInfo().WithClearedConcurrency());
            }

            if (interaction.Facets.ContainsKey(ProfileScores.DefaultFacetKey))
            {
                _client.SetFacet(
                    result,
                    ProfileScores.DefaultFacetKey,
                    interaction.ProfileScores().WithClearedConcurrency());
            }

            if (interaction.Facets.ContainsKey(WebVisit.DefaultFacetKey))
            {
                _client.SetFacet(result, WebVisit.DefaultFacetKey, interaction.WebVisit().WithClearedConcurrency());
            }

            _client.Submit();
        }

        private Contact SearchContact(Contact contact)
        {
            return SearchContactByIdentifier(contact, _config.IdentifierSource)
                   ?? SearchContactByIdentifier(contact, "Alias", "AliasOld")
                   ?? SearchContactByIdentifier(contact, "xDB.Tracker");
        }

        private Contact SearchContactByIdentifier(
            Contact contact,
            string identifierSource,
            string identifierTarget = null)
        {
            Contact result = null;
            if (string.IsNullOrWhiteSpace(identifierTarget))
            {
                identifierTarget = identifierSource;
            }

            ContactIdentifier identifier = contact.Identifiers.FirstOrDefault(i => i.Source == identifierSource);
            if (identifier != null)
            {
                result = _client.Get(
                    new IdentifiedContactReference(identifierTarget, identifier.Identifier),
                    GetExpandOptions());
            }

            Log.Logger.Debug($"Searching contact {contact.Id} by identifier {identifierSource} and found {result?.Id}");
            return result;
        }

        private ContactExpandOptions GetExpandOptions()
        {
            return new ContactExpandOptions(_config.ContactFacets)
                       {
                           Interactions = new RelatedInteractionsExpandOptions(_config.InteractionFacets)
                                              {
                                                  StartDateTime = DateTime.MinValue,
                                                  EndDateTime = DateTime.MaxValue,
                                                  Limit = int.MaxValue
                                              }
                       };
        }
    }
}