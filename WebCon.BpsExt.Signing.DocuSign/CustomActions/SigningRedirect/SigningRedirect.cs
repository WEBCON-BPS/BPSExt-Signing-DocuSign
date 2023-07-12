using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SigningRedirect
{
    public class SigningRedirect : CustomAction<SigningRedirectConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override Task RunAsync(RunCustomActionParams args)
        {
            var returnUrl = Configuration.RedirectUrl;
            ConnectionsHelper connectionsHelper = new ConnectionsHelper(args.Context);
            var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, connectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));

            var embededUserInfo = new EmbededUserModel()
            {
                Name = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededUserNameFieldId).GetValue().ToString(),
                Mail = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededUserMailFieldId).GetValue().ToString(),
                RecipientId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededRecipientIdFieldId).GetValue().ToString(),
                ClientUserId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededClientUserIdFieldId).GetValue().ToString(),
                EnvelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EnvelopeGUIDFieldId).GetValue().ToString()
            };
            var view = new ApiHelper(apiClient, connectionsHelper, Configuration.ApiSettings, _logger).CreateRecipientView(embededUserInfo, returnUrl);
            args.TransitionInfo.RedirectUrl(view.Url);
            return Task.CompletedTask;
        }
    }

    public class EmbededUserModel
    {
        public string Name { get; set; }
        public string Mail { get; set; }
        public string RecipientId { get; set; }
        public string ClientUserId { get; set; }
        public string EnvelopeId { get; set; }
    }
}