using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendReminder
{
    public class SendReminderConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Input parameters")]
        public EnvelopeSettings EnvelopeSettings { get; set; }
    }

    public class EnvelopeSettings
    {
        [ConfigEditableFormFieldID(DisplayName = "Envelope ID",
            Description = "Select the text field where the Envelope ID was saved.", IsRequired = true)]
        public int EnvelopeGUIDFieldId { get; set; }
    }
}