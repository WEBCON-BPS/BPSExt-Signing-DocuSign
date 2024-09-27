using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendReminder
{
    public class SendReminder : CustomAction<SendReminderConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var apiClient = new DocuSignClient();
                var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EnvelopeSettings.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Sending reminder to recipients of envelope: {envelopeId}");
                await new ApiHelper(apiClient, Configuration.ApiSettings, _logger).ResendEnvelopeAsync(envelopeId);
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