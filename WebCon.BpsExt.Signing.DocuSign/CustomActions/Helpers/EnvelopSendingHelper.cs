using DocuSign.eSign.Model;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    public class EnvelopSendingHelper
    {
        private readonly StringBuilder _logger;
        private SendEnvelopeConfigurationBase Configuration;
        private bool UseSms;

        public EnvelopSendingHelper(StringBuilder logger, SendEnvelopeConfigurationBase config, bool useSms)
        {
            _logger = logger;
            Configuration = config;
            UseSms = useSms;
        }

        public async Task<string> CompleteEnvelopeDataAsync(EnvelopeDefinition env, List<AttachmentData> documents, List<SignerData> signers)
        {
            var creatingResult = await CreateDocumentsAsync(documents);
            var recps = CreateSigners(signers);
            var templates = new List<CompositeTemplate>();
            templates.Add(new CompositeTemplate()
            {
                InlineTemplates = new List<InlineTemplate>()
                      {
                          new InlineTemplate()
                          {
                               Sequence = "1",
                               Recipients = recps,
                               Documents = creatingResult.documents
                          }
                      }
            });
            env.CompositeTemplates = templates;
            _logger.AppendLine("Envelope data completed");
            return creatingResult.documentsInfoToSave;
        }

        public Recipients CreateSigners(List<SignerData> signersData)
        {
            _logger.AppendLine("Adding signers to Envelope");
            var signers = new List<Signer>();
            foreach (var signerData in signersData.Select((value, i) => new { i, value }))
            {
                var signer = CreateSigner(signerData.value, signerData.i + 1);
                signers.Add(signer);
            }
            var recipients = new Recipients();
            recipients.Signers = signers;
            return recipients;
        }

        private Signer CreateSigner(SignerData signerData, int recipientId)
        {
            var signer = new Signer()
            {
                Name = signerData.Name,
                Email = signerData.Mail,
                Status = "Created",
                DeliveryMethod = "Email",
                RecipientId = recipientId.ToString(),
                RequireIdLookup = "false",
            };
           
            if (UseSms && !string.IsNullOrEmpty(signerData.PhoneNumber))
            {
                var smsAuth = new RecipientSMSAuthentication();
                smsAuth.SenderProvidedNumbers = new List<String>() { signerData.PhoneNumber };
                signer.RequireIdLookup = "true";
                signer.IdCheckConfigurationName = "SMS Auth $";
                signer.SmsAuthentication = smsAuth;
            }
            return signer;
        }

        private async Task<(List<Document> documents, string documentsInfoToSave)> CreateDocumentsAsync(List<AttachmentData> docs)
        {
            _logger.AppendLine("Adding documents to Envelope");
            var documents = new List<Document>();
            var documentsInfoToSave = string.Empty;
            foreach (var doc in docs.Select((value, i) => new { i, value }))
            {
                var content = await doc.value.GetContentAsync();
                documents.Add(new Document()
                {
                    DocumentBase64 = Convert.ToBase64String(content),
                    DocumentId = (doc.i + 1).ToString(),
                    FileExtension = doc.value.FileExtension,
                    Name = doc.value.FileName,
                    TransformPdfFields = true.ToString()
                });
                documentsInfoToSave += $"{doc.value.ID}#{doc.i + 1};";
            }
            return (documents, documentsInfoToSave);
        }
    }
}


