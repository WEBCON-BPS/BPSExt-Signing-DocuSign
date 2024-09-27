using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Text;
using DocuSign.eSign.Api;
using System.IO;
using static DocuSign.eSign.Api.EnvelopesApi;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.SigningRedirect;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    internal class ApiHelper : ApiHelperBase
    {
        public ApiHelper(DocuSignClient apiClient, ApiConfigurationBase config, StringBuilder logger ) : base(apiClient, config, logger)
        {
        }

        public async Task<EnvelopeSummary> SendEnvelopeAsync(EnvelopeDefinition envelope)
        {
            var envelopeApi = PrepareApi();
            var results = await envelopeApi.CreateEnvelopeAsync(AccountID, envelope);
            return results;
        }

        private EnvelopesApi PrepareApi()
        {
           CheckToken();
           return new EnvelopesApi(ApiClient);
        }

        public async Task<Envelope> GetEnvelopAsync(string envelopeId)
        {
            var envelopeApi = PrepareApi();
            return await envelopeApi.GetEnvelopeAsync(AccountID, envelopeId);
        }

        public async Task<EnvelopeUpdateSummary> BlockEnvelopeAsync(string envelopeId)
        {          
            var envelope =  new Envelope();
            var envelopeApi = PrepareApi();
            envelope.Status = "voided";
            envelope.VoidedReason = "Deleted";
            return await envelopeApi.UpdateAsync(AccountID, envelopeId, envelope);
        }

        public async Task<RecipientsUpdateSummary> ResendEnvelopeAsync(string envelopeId)
        {
            var recipients = await GetRecipientsAsync(envelopeId);
            var envelopeApi = PrepareApi();
            var options = new UpdateRecipientsOptions()
            {
                resendEnvelope = true.ToString()
            };
            return await envelopeApi.UpdateRecipientsAsync(AccountID, envelopeId, recipients, options);
        }

        internal async Task<Recipients> GetRecipientsAsync(string envelopeId)
        {
            var envelopeApi = PrepareApi();
            return await envelopeApi.ListRecipientsAsync(AccountID, envelopeId);
        }

        public async Task<List<DocumentFromEnvelope>> DownloadDocumentsAsync(string envelopeId)
        {
            var documents = await ListDocumentsAsync(envelopeId);
            var envelopesApi = PrepareApi();
            var downloadedDocuments = new List<DocumentFromEnvelope>();
            foreach (var doc in documents)
            {
                if (doc.DocumentId == "certificate")
                    continue;

                var results = await envelopesApi.GetDocumentAsync(AccountID, envelopeId, doc.DocumentId);
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
            var envelopesApi = PrepareApi();
            var results = await envelopesApi.ListDocumentsAsync(AccountID, envelopeId);
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
            var envelopeApi = PrepareApi();
            var options = new ListStatusChangesOptions();
            var date = DateTime.Now.AddDays(-envelopeLifetimeInDays);
            options.fromDate = date.ToString("yyyy/MM/dd");
            return await envelopeApi.ListStatusChangesAsync(AccountID, options);
        }

        public async Task<ViewUrl> CreateRecipientViewAsync(EmbededUserModel embededUserInfo, string returnUrl)
        {
            var envelopeApi = PrepareApi();
            var view = new RecipientViewRequest()
            {
                UserName = embededUserInfo.Name,
                Email = embededUserInfo.Mail,
                RecipientId = embededUserInfo.RecipientId,
                ClientUserId = embededUserInfo.ClientUserId,
                AuthenticationMethod = "email",
                ReturnUrl = returnUrl
            };
            return await envelopeApi.CreateRecipientViewAsync(AccountID, embededUserInfo.EnvelopeId, view);
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
