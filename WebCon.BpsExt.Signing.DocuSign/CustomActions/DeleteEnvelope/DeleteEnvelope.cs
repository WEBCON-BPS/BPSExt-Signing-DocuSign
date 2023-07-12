using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DeleteEnvelope
{
    public class DeleteEnvelope : CustomAction<DeleteEnvelopeConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                ConnectionsHelper connectionsHelper = new ConnectionsHelper(args.Context);
                var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, connectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));
                var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Voiding envelope: {envelopeId}");
                var summary = new ApiHelper(apiClient, connectionsHelper, Configuration.ApiSettings, _logger).BlockEnvelope(envelopeId);
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
            return Task.CompletedTask;
        }
    }
}