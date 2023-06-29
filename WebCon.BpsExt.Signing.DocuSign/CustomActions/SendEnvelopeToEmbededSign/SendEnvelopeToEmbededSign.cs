using System;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using DocuSign.eSign.Model;
using System.Text;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using System.Collections.Generic;
using DocuSign.eSign.Client;
using System.Linq;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelopeToEmbededSign
{
    public class SendEnvelopeToEmbededSign : CustomAction<SendEnvelopeToEmbededSignConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {
                var dataHelper = new DataHelper(_logger, args.Context);
                var documents = await dataHelper.GetDocumentsAsync(Configuration);
                var signers = dataHelper.GetEmbededSigner(Configuration);
                var summary = SendEmails(documents, signers, args.Context);
                args.Context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.EnvelopeFieldId).SetValue(summary.EnvelopeId);
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
            var apiClient = new DocuSignClient();
            _logger.AppendLine("Sending envelope");          
            return new ApiHelper(apiClient, Configuration.ApiSettings, _logger).SendEnvelope(envelope);
        }
        private EnvelopeDefinition CreateEnvelope()
        {
            var env =  new EnvelopeDefinition()
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
            context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.SignerNameFieldId).SetValue(signer.Name);
            context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.SignerMailFieldId).SetValue(signer.Email);
            context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.RecipientIdFieldId).SetValue(signer.RecipientId);
            context.CurrentDocument.Fields.GetByID(Configuration.EmbededSigningConfig.ClientUserIdFieldId).SetValue(signer.ClientUserId);
            context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.TechnicalFieldID).SetValue(documentsInfoToSave);
        }
    }
}