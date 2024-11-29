using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using DocuSign.eSign.Api;
using System.IO;
using static DocuSign.eSign.Api.EnvelopesApi;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.SigningRedirect;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.Common.Model;
using WebCon.WorkFlow.SDK.Tools.Data;
using WebCon.WorkFlow.SDK.Tools.Data.Model;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    internal class ApiHelper
    {
        EnvelopesApi _client;
        string _accountId;


        private ApiHelper(EnvelopesApi client, string accountId)
        {
            _client = client;
            _accountId = accountId;
        }

        public static ApiHelper Create(ApiConfigurationBase config, BaseContext context)
        {
            var helper = new ConnectionsHelper(context);
            var connection = GetConnection(config.ConnectionId, helper);
            var provider = new EnvelopesApiProvider(connection, helper);
            var result = provider.CreateClient(config.UseProxy);
            return new ApiHelper(result.client, result.accountId);
        }

        private static WebServiceConnection GetConnection(int? connectionId, ConnectionsHelper helper)
        {
            if(connectionId.HasValue)
                return helper.GetConnectionToWebService(new GetByConnectionParams(connectionId.Value));

            throw new Exception(@"No connection configuration.
From version 2025 configuration of connection was changed to standard BPS connection.
Integration Key -> Client ID
Impersonated User Guid -> User
Auth Server -> Authentication service URL
Private Key -> Client Secret
");
        }

        public async Task<EnvelopeSummary> SendEnvelopeAsync(EnvelopeDefinition envelope)
        {
            var results = await _client.CreateEnvelopeAsync(_accountId, envelope);
            return results;
        }

        public async Task<Envelope> GetEnvelopAsync(string envelopeId)
        {
            return await _client.GetEnvelopeAsync(_accountId, envelopeId);
        }

        public async Task<EnvelopeUpdateSummary> BlockEnvelopeAsync(string envelopeId)
        {          
            var envelope =  new Envelope();
            envelope.Status = "voided";
            envelope.VoidedReason = "Deleted";
            return await _client.UpdateAsync(_accountId, envelopeId, envelope);
        }

        public async Task<RecipientsUpdateSummary> ResendEnvelopeAsync(string envelopeId)
        {
            var recipients = await GetRecipientsAsync(envelopeId);
            var options = new UpdateRecipientsOptions()
            {
                resendEnvelope = true.ToString()
            };
            return await _client.UpdateRecipientsAsync(_accountId, envelopeId, recipients, options);
        }

        internal async Task<Recipients> GetRecipientsAsync(string envelopeId)
        {
            return await _client.ListRecipientsAsync(_accountId, envelopeId);
        }

        public async Task<List<DocumentFromEnvelope>> DownloadDocumentsAsync(string envelopeId)
        {
            var documents = await ListDocumentsAsync(envelopeId);
            var downloadedDocuments = new List<DocumentFromEnvelope>();
            foreach (var doc in documents)
            {
                if (doc.DocumentId == "certificate")
                    continue;
                var results = await _client.GetDocumentAsync(_accountId, envelopeId, doc.DocumentId);
                string docName = doc.Name;
                bool hasPDFsuffix = docName.ToUpper().EndsWith(".PDF");
                bool pdfFile = hasPDFsuffix;
                string docType = doc.Type;
                if (("content".Equals(docType) || "summary".Equals(docType)) && !hasPDFsuffix)
                {
                    docName += ".pdf";
                    pdfFile = true;
                }
                if ("zip".Equals(docType))
                {
                    docName += ".zip";
                }

                    using (var saveStream = new MemoryStream())
                    {
                        results.CopyTo(saveStream);
                        downloadedDocuments.Add(new DocumentFromEnvelope()
                        {
                            DocumentContent = saveStream.ToArray(),
                            Name = docName,
                            DocumentID = Int32.Parse(doc.DocumentId)
                        });
                    }
            }
            return downloadedDocuments;
        }

        internal async Task<List<EnvelopeDocItem>> ListDocumentsAsync(string envelopeId)
        {
            var results = await _client.ListDocumentsAsync(_accountId, envelopeId);
            var envelopeDocItems = new List<EnvelopeDocItem>();

            foreach (EnvelopeDocument doc in results.EnvelopeDocuments)
            {
                envelopeDocItems.Add(new EnvelopeDocItem
                {
                    DocumentId = doc.DocumentId,
                    Name = doc.DocumentId == "certificate" ? "Certificate of completion" : doc.Name,
                    Type = doc.Type
                });
            }
            return envelopeDocItems;
        }

        public async Task<EnvelopesInformation> ListChangesAsync(int envelopeLifetimeInDays)
        {
            var options = new ListStatusChangesOptions();
            var date = DateTime.Now.AddDays(-envelopeLifetimeInDays);
            options.fromDate = date.ToString("yyyy/MM/dd");
            return await _client.ListStatusChangesAsync(_accountId, options);
        }

        public async Task<ViewUrl> CreateRecipientViewAsync(EmbededUserModel embededUserInfo, string returnUrl)
        {
            var view = new RecipientViewRequest()
            {
                UserName = embededUserInfo.Name,
                Email = embededUserInfo.Mail,
                RecipientId = embededUserInfo.RecipientId,
                ClientUserId = embededUserInfo.ClientUserId,
                AuthenticationMethod = "email",
                ReturnUrl = returnUrl
            };
            return await _client.CreateRecipientViewAsync(_accountId, embededUserInfo.EnvelopeId, view);
        }


        public class EnvelopeDocItem
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string DocumentId { get; set; }
        }

        public class DocumentFromEnvelope
        {
            public byte[] DocumentContent { get; set; }
            public string Name { get; set; }
            public int DocumentID { get; set; }
        }
    }
}
