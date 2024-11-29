using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.Common.Model;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelope
{
    public class SendEnvelope : CustomAction<SendEnvelopeConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override async Task RunAsync(RunCustomActionParams args)
        {
            try
            {           
                var dataHelper = new DataHelper(_logger, args.Context);
                var documents = await dataHelper.GetDocumentsAsync(Configuration);
                var signers = dataHelper.GetSigners(Configuration);
               
                var summary = await SendEmailsAsync(documents, signers, args.Context);
                await SetFieldsAsync(summary, args.Context);           
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

        private async Task SetFieldsAsync(Tuple<EnvelopeSummary, string> summary, ActionContextInfo context)
        {
            await context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.EnvelopeFieldId).SetValueAsync(summary.Item1.EnvelopeId);
            await context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.TechnicalFieldID).SetValueAsync(summary.Item2);
        }

        private async Task<Tuple<EnvelopeSummary, string>> SendEmailsAsync(List<AttachmentData> documents, List<SignerData> signers, BaseContext context)
        {          
            var envelope = CreateEnvelope();
            var sendHelper = new EnvelopSendingHelper(_logger, Configuration, Configuration.RecipientsSelection.UseSMS);
            var documentsInfoToSave = await sendHelper.CompleteEnvelopeDataAsync(envelope, documents, signers);
            _logger.AppendLine("Sending envelope");
            var helper = ApiHelper.Create(Configuration.ApiSettings, context);
            var envelopeResult = await helper.SendEnvelopeAsync(envelope);
            var result = new Tuple<EnvelopeSummary, string>(envelopeResult, documentsInfoToSave);
            return result;
        }
        private EnvelopeDefinition CreateEnvelope()
        {
            return new EnvelopeDefinition()
            {
                EnvelopeIdStamping = "true",
                EmailSubject = Configuration.MessageContent.MailSubject,
                EmailBlurb = Configuration.MessageContent.MailBody,
                Status = "Sent"
            };
        }
    }
}
