using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DeleteEnvelope
{
    public class DeleteEnvelope : CustomAction<DeleteEnvelopeConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Voiding envelope: {envelopeId}");
                var helper = ApiHelper.Create(Configuration.ApiSettings, args.Context);
                await helper.BlockEnvelopeAsync(envelopeId);
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