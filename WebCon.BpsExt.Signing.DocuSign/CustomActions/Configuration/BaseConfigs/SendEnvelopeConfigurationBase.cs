using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration
{
    public class SendEnvelopeConfigurationBase : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "DocuSign API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Output parameters")]
        public SendDocumentOutputParameters OutputParameters { get; set; }

        [ConfigGroupBox(DisplayName = "Attachment selection")]
        public AttachmentSelection AttachmentSelection { get; set; }
    }


    public class AttachmentSelection
    {
        [ConfigEditableEnum(DisplayName = "Selection mode",
            Description = "The attachments to sign can be selected by category ID and regex (optional) or by SQL query")]
        public AttachmentsChoosingOptions AttachmentsChoosingOption { get; set; }

        [ConfigEditableEnum(DisplayName = "Category mode", Description = "Select None for files not associated with any category or All for attachment from the element")]
        public CategorySelectionOptions CategorySelectionOptions { get; set; }

        [ConfigEditableText(DisplayName = "Category ID",
            Description = "Select the attachment category to be signed. If 'Category mode' is set to 'ID' this field must be set")]
        public string GroupID { get; set; }

        [ConfigEditableText(DisplayName = "Regex expression",
            Description = "Regular expression can be used as an additional filter for attachments from the selected category")]
        public string AttRegularExpression { get; set; }

        [ConfigEditableText(DisplayName = "SQL query", Multiline = true, TagEvaluationMode = EvaluationMode.SQL,
            Description = "Query should return a list of attachments IDs from WFDataAttachmets table. Example: Select [ATT_ID] from [WFDataAttachmets] Where [ATT_Name] = 'agreement.pdf' ")]
        public string AttQuery { get; set; }
    }


    public class SignersList : IConfigEditableItemList
    {
        public int ItemListId { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Name", IsRequired = true)]
        public int SignerNameColumnID { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "E-mail", IsRequired = true)]
        public int SignerMailColumnID { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Phone Number", IsRequired = true)]
        public int SignerPhoneNumberColumnID { get; set; }

    }

    public enum AttachmentsChoosingOptions
    {
        Category = 0,
        SQL = 1
    }

    public enum CategorySelectionOptions
    {
        ID = 0,
        All = 1,
        None = 2
    }
}
