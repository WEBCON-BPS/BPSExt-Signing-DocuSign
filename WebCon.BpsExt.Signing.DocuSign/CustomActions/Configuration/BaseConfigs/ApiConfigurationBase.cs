using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration
{
    public class ApiConfigurationBase
    {

        [ConfigEditableText(DisplayName = "Integration Key", IsRequired = true,
            Description = "Integration Key associated with used DocuSign REST API app")]
        public string ClientID { get; set; }

        [ConfigEditableText(DisplayName = "Impersonated User Guid", IsRequired = true,
            Description = "Client ID of user on which app will be impersonating")]
        public string ImpersonatedUserGuid { get; set; }

        [ConfigEditableText(DisplayName = "Auth Server", IsRequired = true,
            Description = "DEV/TEST = account-d.docusign.com, PROD = account.docusign.com")]
        public string AuthServer { get; set; }

        [ConfigEditableText(DisplayName = "Private Key", IsRequired = true, Multiline = true, 
            Description = "Private Key associated with used DocuSign REST API app")]
        public string PrivateKey { get; set; }

    }
}
