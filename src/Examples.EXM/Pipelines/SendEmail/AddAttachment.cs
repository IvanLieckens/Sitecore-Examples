using System;
using System.Collections.Generic;

using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.EDS.Core.Dispatch;
using Sitecore.EmailCampaign.Cm.Pipelines.SendEmail;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.XConnect;

namespace Examples.EXM.Pipelines.SendEmail
{
    public class AddAttachment
    {
        private readonly List<ID> _attachToMessageIdList = new List<ID>();

        private readonly IContactService _contactService;

        public AddAttachment(IContactService contactService)
        {
            _contactService = contactService;
        }

        public void Process(SendMessageArgs args)
        {
            // NOTE [ILs] This is the message Sitecore item wrapped for easy access to various details
            MailMessageItem message = args.EcmMessage as MailMessageItem;
            if (_attachToMessageIdList.Contains(message?.InnerItem.ID))
            {
                // NOTE [ILs] The actual EDS core message that will be dispatched is hidden in custom data
                if (args.CustomData["EmailMessage"] is EmailMessage email)
                {
                    FileResource attachment = GenerateAttachment(message);
                    email.Attachments.Add(attachment);
                }
            }
        }

        /// <summary>
        /// This method allows adding message ids through configuration ensuring no recompile is needed when you need to alter your messages that use this functionality.
        /// </summary>
        public void AddMessageToAttachTo(string id)
        {
            if (Guid.TryParse(id, out Guid guid))
            {
                _attachToMessageIdList.Add(new ID(guid));
            }
            else
            {
                Log.Error("Invalid GUID detected to be added to the Attach To list for the AddAttachment processor of the SendEmail pipeline.", this);
            }
        }

        /// <summary>
        /// This method generated a FileResource to attach to the email.
        /// </summary>
        private FileResource GenerateAttachment(MailMessageItem message)
        {
            // NOTE [ILs] To know who you're generating for you can fetch the XConnect contact (or use the other data available)
            Contact contact = _contactService.GetContact(message.ContactIdentifier, "pass desired facet keys you need");

            // TODO [ILs] Implement your attachment generation code here

            // NOTE [ILs] You can generate any desired attachment here and append the bytes
            return new FileResource("Attachment name here", new byte[0]);
        }
    }
}