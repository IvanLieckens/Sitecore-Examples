using System.Linq;

using Sitecore.EDS.Core.Dispatch;
using Sitecore.EmailCampaign.Cm.Pipelines.SendEmail;
using Sitecore.EmailCampaign.Model;

namespace Examples.EXM.Pipelines.SendEmail
{
    public class ModifySenderHeader
    {
        public void Process(SendMessageArgs args)
        {
            // NOTE [ILs] The actual EDS core message that will be dispatched is hidden in custom data
            if (args.CustomData["EmailMessage"] is EmailMessage email)
            {
                if (email.Headers.AllKeys.Contains(Constants.SenderField))
                {
                    email.Headers[Constants.SenderField] = "\"Automated System\" <automatedsystem@customer.com>";
                }
            }
        }
    }
}