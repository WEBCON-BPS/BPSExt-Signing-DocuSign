using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelopeToEmbededSign
{
	public class SendEnvelopeToEmbededSign : CustomAction<SendEnvelopeToEmbededSignConfig>
	{
		private ConnectionsHelper ConnectionsHelper;
		readonly StringBuilder _logger = new StringBuilder();
		public override async Task RunAsync(RunCustomActionParams args)
		{
			try
			{
				ConnectionsHelper = new ConnectionsHelper(args.Context);
				var dataHelper = new DataHelper(_logger, args.Context);
				var documents = await dataHelper.GetDocumentsAsync(Configuration);
				var signers = dataHelper.GetEmbededSigner(Configuration);
				var summary = SendEmails(documents, signers, args.Context);
				args.Context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.EnvelopeFieldId).SetValueAsync(summary.EnvelopeId).Wait();
			}
			catch (ApiException e)
			{
				var message = e.Message;
				if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
				{
					_logger.AppendLine("CONSENT REQUIRED");
					args.Message = "CONSENT REQUIRED";
				}
				else
				{
					_logger.AppendLine(e.ToString());
					args.Message = e.Message;
				}
				args.HasErrors = true;
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

		private EnvelopeSummary SendEmails(List<AttachmentData> documents, List<SignerData> signer, ActionContextInfo context)
		{
			var envelope = CreateEnvelope();
			var sendHelper = new EnvelopSendingHelper(_logger, Configuration, Configuration.RecipientSelection.UseSMS);
			sendHelper.CompleteEnvelopeData(envelope, documents, signer, out string documentsInfoToSave);
			envelope.CompositeTemplates.FirstOrDefault().InlineTemplates.FirstOrDefault().Recipients.Signers.FirstOrDefault().ClientUserId = Guid.NewGuid().ToString();
			SaveEmbededInfoOnForm(context, envelope, documentsInfoToSave);
			var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, ConnectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));
			_logger.AppendLine("Sending envelope");
			return new ApiHelper(apiClient, ConnectionsHelper, Configuration.ApiSettings, _logger).SendEnvelope(envelope);
		}
		private EnvelopeDefinition CreateEnvelope()
		{
			var env = new EnvelopeDefinition()
			{
				EnvelopeIdStamping = "true",
				Status = "Sent",
				EmailSubject = Configuration.MessageContent.MessageSubjectContent
			};
			return env;
		}

		private void SaveEmbededInfoOnForm(ActionContextInfo context, EnvelopeDefinition envelop, string documentsInfoToSave)
		{
			var signer = envelop.CompositeTemplates.First().InlineTemplates.First().Recipients.Signers.First();
			context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.SignerNameFieldId).SetValueAsync(signer.Name).Wait();
			context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.SignerMailFieldId).SetValueAsync(signer.Email).Wait();
			context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.RecipientIdFieldId).SetValueAsync(signer.RecipientId).Wait();
			context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.ClientUserIdFieldId).SetValueAsync(signer.ClientUserId).Wait();
			context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.TechnicalFieldID).SetValueAsync(documentsInfoToSave).Wait();
		}
	}
}