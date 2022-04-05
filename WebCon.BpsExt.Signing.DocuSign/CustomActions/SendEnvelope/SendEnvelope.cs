using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelope
{
    public class SendEnvelope : CustomAction<SendEnvelopeConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override void Run(RunCustomActionParams args)
        {
            try
            {           
                var dataHelper = new DataHelper(_logger, args.Context);
                var documents = dataHelper.GetDocuments(Configuration);
                var signers = dataHelper.GetSigners(Configuration);
               
                var summary = SendEmails(documents, signers);
                SetFields(summary, args.Context);           
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
                args.Context.PluginLogger?.AppendInfo(_logger.ToString());
            }
        }

        private void SetFields(Tuple<EnvelopeSummary, string> summary, ActionContextInfo context)
        {
            context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.EnvelopeFieldId).SetValue(summary.Item1.EnvelopeId);
            context.CurrentDocument.Fields.GetByID(Configuration.OutputParameters.TechnicalFieldID).SetValue(summary.Item2);
        }

        private Tuple<EnvelopeSummary, string> SendEmails(List<AttachmentData> documents, List<SignerData> signers)
        {          
            var envelope = CreateEnvelope();
            var sendHelper = new EnvelopSendingHelper(_logger, Configuration, Configuration.RecipientsSelection.UseSMS);
            sendHelper.CompleteEnvelopeData(envelope, documents, signers, out string documentsInfoToSave);
            var apiClient = new ApiClient();
            _logger.AppendLine("Sending envelope");
            var apiHelper = new ApiHelper(apiClient, Configuration.ApiSettings, _logger);
            var result = new Tuple<EnvelopeSummary, string>(apiHelper.SendEnvelope(envelope), documentsInfoToSave);
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
