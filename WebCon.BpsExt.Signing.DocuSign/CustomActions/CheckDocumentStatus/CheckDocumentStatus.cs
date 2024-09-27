using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.CheckDocumentStatus
{
    public class CheckDocumentStatus : CustomAction<CheckDocumentStatusConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var env = await GetEnvelopeAsync(args.Context);
                _logger.AppendLine("Setting field");
                await args.Context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.StatusFieldId).SetValueAsync(env.Status);
            }
            catch (Exception ex)
            {
                _logger.AppendLine(ex.ToString());
                args.Message = ex.Message;
                args.HasErrors = true;
            }
            finally
            {
                args.LogMessage = _logger.ToString();
                args.Context.PluginLogger.AppendInfo(_logger.ToString());           
            }
        }

        private async Task<Envelope> GetEnvelopeAsync(ActionContextInfo context)
        {
            var envId = context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.EnvelopeFieldId).GetValue().ToString();
            var apiClient = new DocuSignClient();
            _logger.AppendLine("Downloading envelope");
            return await new ApiHelper(apiClient, Configuration.ApiSettings, _logger).GetEnvelopAsync(envId);
        }
    }
}