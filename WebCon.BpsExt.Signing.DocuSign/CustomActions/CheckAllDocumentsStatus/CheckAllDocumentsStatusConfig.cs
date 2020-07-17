using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.CheckAllDocumentsStatus
{
    public class CheckAllDocumentsStatusConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "API Settings", Description = "More information : https://developers.docusign.com/esign-rest-api/guides/authentication/oauth2-jsonwebtoken")]
        public ApiConfigurationBase ApiSettings { get; set; }

        [ConfigGroupBox(DisplayName = "Workflow section")]
        public WorkflowConfig Workflow { get; set; }

        [ConfigEditableInteger(DisplayName = "Maximum execution time in seconds", DefaultValue = 120, IsRequired = true)]
        public int MaxExecutionTime { get; set; }

        [ConfigEditableInteger(DisplayName = "Maximum envelope lifetime in days", DefaultValue = 30, IsRequired = true,
            Description = "If 30, only envelopes that are not older than 30 days will be checked")]
        public int EnvelopeLifetimeInDays { get; set; }
    }

    public class WorkflowConfig
    {
        [ConfigEditableText(DisplayName = "Step ID", IsRequired = true)]
        public int StepId { get; set; }

        [ConfigEditableText(DisplayName = "Success Path ID", IsRequired = true)]
        public int SuccessPathId { get; set; }

        [ConfigEditableText(DisplayName = "Incorrect Path ID", IsRequired = true)]
        public int ErrorPathId { get; set; }

        [ConfigEditableText(DisplayName = "Envelope ID field name", IsRequired = true,
            Description = "Database name of field where is saved envelope ID")]
        public string OperationFieldName { get; set; }
    }


}