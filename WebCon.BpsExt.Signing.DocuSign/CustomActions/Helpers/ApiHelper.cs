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

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    internal class ApiHelper : ApiHelperBase
    {
        public ApiHelper(ApiClient apiClient, ApiConfigurationBase config, StringBuilder logger ) : base(apiClient, config, logger)
        {
        }

        public EnvelopeSummary SendEnvelope(EnvelopeDefinition envelope)
        {
            var envelopeApi = PrepareApi();
            var results = envelopeApi.CreateEnvelope(AccountID, envelope);
            return results;
        }

        private EnvelopesApi PrepareApi()
        {
           CheckToken();
           return new EnvelopesApi(ApiClient);
        }

        public Envelope GetEnvelop(string envelopeId)
        {
            var envelopeApi = PrepareApi();
            return envelopeApi.GetEnvelope(AccountID, envelopeId);
        }

        public EnvelopeUpdateSummary BlockEnvelope(string envelopeId)
        {          
            var envelope =  new Envelope();
            var envelopeApi = PrepareApi();
            envelope.Status = "voided";
            envelope.VoidedReason = "Deleted";
            return envelopeApi.Update(AccountID, envelopeId, envelope);
        }

        public RecipientsUpdateSummary ResendEnvelope(string envelopeId)
        {
            var recipients = GetRecipients(envelopeId);
            var envelopeApi = PrepareApi();
            var options = new UpdateRecipientsOptions()
            {
                resendEnvelope = true.ToString()
            };
            return envelopeApi.UpdateRecipients(AccountID, envelopeId, recipients, options);
        }

        internal Recipients GetRecipients(string envelopeId)
        {
            var envelopeApi = PrepareApi();
            return envelopeApi.ListRecipients(AccountID, envelopeId);
        }

        public List<DocumentFromEnvelope> DownloadDocuments(string envelopeId)
        {
            var documents = ListDocuments(envelopeId);
            var envelopesApi = PrepareApi();
            var downloadedDocuments = new List<DocumentFromEnvelope>();
            foreach (var doc in documents)
            {
                if (doc.DocumentId == "certificate")
                    continue;

                var results = envelopesApi.GetDocument(AccountID, envelopeId, doc.DocumentId);
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

        internal List<EnvelopeDocItem> ListDocuments(string envelopeId)
        {
            var envelopesApi = PrepareApi();
            var results = envelopesApi.ListDocuments(AccountID, envelopeId);
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

        public EnvelopesInformation ListChanges(int envelopeLifetimeInDays)
        {
            var envelopeApi = PrepareApi();
            var options = new ListStatusChangesOptions();
            var date = DateTime.Now.AddDays(-envelopeLifetimeInDays);
            options.fromDate = date.ToString("yyyy/MM/dd");
            return envelopeApi.ListStatusChanges(AccountID, options);
        }

        public ViewUrl CreateRecipientView(EmbededUserModel embededUserInfo, string returnUrl)
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
            return envelopeApi.CreateRecipientView(AccountID, embededUserInfo.EnvelopeId, view);
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
