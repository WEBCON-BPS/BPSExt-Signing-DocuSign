using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SigningRedirect
{
    public class SigningRedirectConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigEditableText(DisplayName = "Url to the form", IsRequired = true)]
        public string RedirectUrl { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Envelope GUID", IsRequired = true)]
        public int EnvelopeGUIDFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Embeded user name", IsRequired = true)]
        public int EmbededUserNameFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Embeded user e-mail", IsRequired = true)]
        public int EmbededUserMailFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Embeded user recipient id", IsRequired = true)]
        public int EmbededRecipientIdFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Embeded user client user id", IsRequired = true)]
        public int EmbededClientUserIdFieldId { get; set; }
    }
}