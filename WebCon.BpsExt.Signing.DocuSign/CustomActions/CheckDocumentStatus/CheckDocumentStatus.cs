using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.CheckDocumentStatus
{
	public class CheckDocumentStatus : CustomAction<CheckDocumentStatusConfig>
	{
		private ConnectionsHelper ConnectionsHelper;
		readonly StringBuilder _logger = new StringBuilder();
		public override Task RunAsync(RunCustomActionParams args)
		{
			try
			{
				ConnectionsHelper = new ConnectionsHelper(args.Context);
				var env = GetEnvelope(args.Context);
				_logger.AppendLine("Setting field");
				args.Context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.StatusFieldId).SetValueAsync(env.Status).Wait();
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

		private Envelope GetEnvelope(ActionContextInfo context)
		{
			var envId = context.CurrentDocument.Fields.GetByID(Configuration.InputParameters.EnvelopeFieldId).GetValue().ToString();
			var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, ConnectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));
			_logger.AppendLine("Downloading envelope");
			return new ApiHelper(apiClient, ConnectionsHelper, Configuration.ApiSettings, _logger).GetEnvelop(envId);
		}
	}
}