using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelopeToEmbededSign
{
    public class SendEnvelopeToEmbededSignConfig : SendEnvelopeConfigurationBase
    {

        [ConfigGroupBox(DisplayName = "Recipient selection")]
        public RecipientSelection RecipientSelection { get; set; }

        [ConfigGroupBox(DisplayName = "Embeded signing configuration")]
        public EmbededSigningConfiguration EmbededSigningConfig { get; set; }

        [ConfigGroupBox(DisplayName = "Message subject")]
        public MessageSubject MessageContent { get; set; }
    }

    public class MessageSubject
    {
        [ConfigEditableText(DisplayName = "Subject", IsRequired = true, Multiline = true)]
        public string MessageSubjectContent { get; set; }
    }

    public class RecipientSelection
    {
        [ConfigEditableText(DisplayName = "Signer Name" ,IsRequired = true)]
        public string SignerName { get; set; }

        [ConfigEditableText(DisplayName = "Signer Mail", IsRequired = true)]
        public string SignerMail { get; set; }

        [ConfigEditableText(DisplayName = "Signer phone number")]
        public string SignerPhoneNumber { get; set; }

        [ConfigEditableBool(DisplayName = "Additional SMS verification",
Description = "Mark this field if additional verification should be required")]
        public bool UseSMS { get; set; }
    }

    public class EmbededSigningConfiguration
    {

        [ConfigEditableFormFieldID(DisplayName = "Field for signer name", IsRequired = true)]
        public int SignerNameFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Field for signer e-mail", IsRequired = true)]
        public int SignerMailFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Field for recipient id", IsRequired = true)]
        public int RecipientIdFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Field for client user id", IsRequired = true)]
        public int ClientUserIdFieldId { get; set; }
    }
}