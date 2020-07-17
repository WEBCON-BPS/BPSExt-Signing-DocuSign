using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelope
{
    public class SendEnvelopeConfig : SendEnvelopeConfigurationBase
    {

        [ConfigGroupBox(DisplayName = "Message content")]
        public MessageContent MessageContent { get; set; }

        [ConfigGroupBox(DisplayName = "Recipients selection")]
        public RecipientsSelection RecipientsSelection { get; set; }

    }

    public class RecipientsSelection
    {
        [ConfigEditableItemList(DisplayName = "Signers Item List")]
        public SignersList SignersList { get; set; }

        [ConfigEditableBool(DisplayName = "Additional SMS verification",
Description = "Mark this field if additional verification should be required")]
        public bool UseSMS { get; set; }
    }
    public class MessageContent
    {
        [ConfigEditableText(DisplayName = "Subject", IsRequired = true, Multiline = true)]
        public string MailSubject { get; set; }

        [ConfigEditableText(DisplayName = "Content", IsRequired = true, Multiline = true,
            Description = "Maximum 10000 characters")]
        public string MailBody { get; set; }
    }

}