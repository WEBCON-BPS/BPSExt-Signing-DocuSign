using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Other;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DownloadDocuments
{
    public class DownloadDocuments : CustomAction<DownloadDocumentsConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override void Run(RunCustomActionParams args)
        {
            try
            {
                var apiClient = new ApiClient();
                var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.DocumentSettings.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Downloading documents for envelope : {envelopeId}");
                var documents = new ApiHelper(apiClient, Configuration.ApiSettings, _logger).DownloadDocuments(envelopeId);
                AddDocumentsToAttachments(documents, args.Context);
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

        private void AddDocumentsToAttachments(List<ApiHelper.DocumentFromEnvelope> documents, ActionContextInfo context)
        {
            var documentsToOverride = context.CurrentDocument.Fields.GetByID(Configuration.DocumentSettings.TechnicalFieldID).GetValue()?.ToString()?.Split(';');
            foreach (var doc in documents)
            {
                var currentDocAttId = documentsToOverride
                    .Where(x => TextHelper.GetPairName(x) == doc.DocumentID.ToString())
                    .Select(x => TextHelper.GetPairId(x)).FirstOrDefault();

                var group = CreateGroup();
                if (string.IsNullOrEmpty(Configuration.Output.Suffix) && !string.IsNullOrEmpty(currentDocAttId))
                {
                    var att = context.CurrentDocument.Attachments.GetByID(Int32.Parse(currentDocAttId));
                    att.Content = doc.DocumentContent;
                    if (group != null)
                        att.FileGroup = group;
                }              
                else
                {
                    var att = new NewAttachmentData(CreateNameWithSuffix(doc.Name), doc.DocumentContent);
                    if (group != null)
                        att.FileGroup = group;
                    context.CurrentDocument.Attachments.AddNew(att);
                }
            }
        }

        private AttachmentsGroup CreateGroup()
        {
            if (string.IsNullOrEmpty(Configuration.Output.GroupName))
                return null;

            return new AttachmentsGroup(
                    TextHelper.GetPairId(Configuration.Output.GroupName),
                    TextHelper.GetPairName(Configuration.Output.GroupName));
        }

        private string CreateNameWithSuffix(string name)
        {
            return $"{Path.GetFileNameWithoutExtension(name)}{Configuration.Output.Suffix}{Path.GetExtension(name)}";
        }
    }
}