using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration
{
    public class ApiConfigurationBase
    {
        [ConfigEditableConnectionID("Connection to docusign", IsRequired = true)]
        public int? ConnectionId { get; set; }

        [ConfigEditableBool("Use proxy")]
        public bool UseProxy { get; set; }
    }
}
