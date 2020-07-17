using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.CheckDocumentStatus
{
    public class CheckDocumentStatusConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Input parameters")]
        public InputParameters InputParameters { get; set; }

    }

    public class InputParameters
    {
        [ConfigEditableFormFieldID(DisplayName = "Copy Envelope ID to field", IsRequired = true,
Description = "Specify a text field on the form where envelope ID will be saved")]
        public int EnvelopeFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Copy Status to field", IsRequired = true,
            Description = "Specify a field on the form where current document status will be saved")]
        public int StatusFieldId { get; set; }
    }


}