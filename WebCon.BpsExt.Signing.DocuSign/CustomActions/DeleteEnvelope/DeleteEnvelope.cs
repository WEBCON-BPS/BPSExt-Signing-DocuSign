using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DeleteEnvelope
{
    public class DeleteEnvelope : CustomAction<DeleteEnvelopeConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var apiClient = new ApiClient();
                var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Voiding envelope: {envelopeId}");
                var summary = new ApiHelper(apiClient, Configuration.ApiSettings, _logger).BlockEnvelope(envelopeId);
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
    }
}