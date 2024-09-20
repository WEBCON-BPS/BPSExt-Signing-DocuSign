using DocuSign.eSign.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Other;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.DownloadDocuments
{
	public class DownloadDocuments : CustomAction<DownloadDocumentsConfig>
	{
		readonly StringBuilder _logger = new StringBuilder();
		public override async Task RunAsync(RunCustomActionParams args)
		{
			try
			{
				ConnectionsHelper connectionsHelper = new ConnectionsHelper(args.Context);
				var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, connectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));
				var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.DocumentSettings.EnvelopeGUIDFieldId).GetValue().ToString();
				_logger.AppendLine($"Downloading documents for envelope : {envelopeId}");
				var documents = new ApiHelper(apiClient, connectionsHelper, Configuration.ApiSettings, _logger).DownloadDocuments(envelopeId);
				await AddDocumentsToAttachmentsAsync(documents, args.Context);
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

		private async Task AddDocumentsToAttachmentsAsync(List<ApiHelper.DocumentFromEnvelope> documents, ActionContextInfo context)
		{
			var manager = new DocumentAttachmentsManager(context);
			var documentsToOverride = context.CurrentDocument.Fields.GetByID(Configuration.DocumentSettings.TechnicalFieldID).GetValue()?.ToString()?.Split(';');
			foreach (var doc in documents)
			{
				var currentDocAttId = documentsToOverride
					.Where(x => TextHelper.GetPairName(x) == doc.DocumentID.ToString())
					.Select(x => TextHelper.GetPairId(x)).FirstOrDefault();

				if (string.IsNullOrEmpty(Configuration.Output.Suffix) && !string.IsNullOrEmpty(currentDocAttId))
				{
					var att = await context.CurrentDocument.Attachments.GetByIDAsync(Int32.Parse(currentDocAttId));
					att.SetContent(doc.DocumentContent);
					if (!string.IsNullOrEmpty(Configuration.Output.GroupName))
						await SetFileGroup(att, Configuration.Output.GroupName);
				}
				else
				{

					var att = await manager.GetNewAttachmentAsync(CreateNameWithSuffix(doc.Name), doc.DocumentContent);
					if (!string.IsNullOrEmpty(Configuration.Output.GroupName))
						await SetFileGroup(att, Configuration.Output.GroupName);

					await context.CurrentDocument.Attachments.AddNewAsync(att);
				}
			}
		}

		private async Task SetFileGroup(NewAttachmentData newAtt, string category)
		{
			if (category.Contains("#"))
			{
				newAtt.FileGroup = new AttachmentsGroup(TextHelper.GetPairId(category), TextHelper.GetPairName(category));
				return;
			}

			var fileGroup = await newAtt.ResolveAsync(category);
			newAtt.FileGroup = fileGroup ?? new AttachmentsGroup(category);
		}

		private string CreateNameWithSuffix(string name)
		{
			return $"{Path.GetFileNameWithoutExtension(name)}{Configuration.Output.Suffix}{Path.GetExtension(name)}";
		}
	}
}