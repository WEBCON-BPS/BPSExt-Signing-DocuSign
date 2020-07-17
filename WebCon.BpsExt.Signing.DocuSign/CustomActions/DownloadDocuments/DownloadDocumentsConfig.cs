using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DownloadDocuments
{
    public class DownloadDocumentsConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "DocuSign API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Document settings")]
        public DocumentSettings DocumentSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Output parameters")]
        public Output Output { get; set; }

    }


    public class DocumentSettings
    {
        [ConfigEditableFormFieldID(DisplayName = "Envelope ID", IsRequired = true,
            Description = "Select the text field where the Envelope ID was saved.")]
        public int EnvelopeGUIDFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Sent Document ID", IsRequired = true,
    Description = "Select the text field where the Sent Documents ID was saved")]
        public int TechnicalFieldID { get; set; }
    }

    public class Output
    {
        [ConfigEditableText(DisplayName = "Category", Description = "Attachment category where the signed documents will be downloaded.")]
        public string GroupName { get; set; }

        [ConfigEditableText(DisplayName = "Suffix", DefaultText = "_sign",
    Description = @"Suffix that will be added to the name of the downloaded file.
When this field is empty then the attachment will be overwritten (if the attachment with the selected Document ID exists on the form).")]
        public string Suffix { get; set; }
    }
}