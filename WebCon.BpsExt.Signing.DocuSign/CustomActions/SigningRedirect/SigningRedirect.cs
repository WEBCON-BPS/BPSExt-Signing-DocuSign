using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SigningRedirect
{
    public class SigningRedirect : CustomAction<SigningRedirectConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            var returnUrl = Configuration.RedirectUrl;
            var apiClient = new DocuSignClient();

            var embededUserInfo = new EmbededUserModel()
            {
                Name = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededUserNameFieldId).GetValue().ToString(),
                Mail = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededUserMailFieldId).GetValue().ToString(),
                RecipientId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededRecipientIdFieldId).GetValue().ToString(),
                ClientUserId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EmbededClientUserIdFieldId).GetValue().ToString(),
                EnvelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EnvelopeGUIDFieldId).GetValue().ToString()
            };
            var view = await  new ApiHelper(apiClient, Configuration.ApiSettings, _logger).CreateRecipientViewAsync(embededUserInfo, returnUrl);
            args.TransitionInfo.RedirectUrl(view.Url);
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