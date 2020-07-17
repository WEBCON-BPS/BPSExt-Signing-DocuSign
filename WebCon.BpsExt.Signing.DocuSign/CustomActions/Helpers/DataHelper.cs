using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelopeToEmbededSign;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.SendEnvelope;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model.Attachments;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    public class DataHelper
    {
        StringBuilder _logger;
        ActionContextInfo Context;
        public DataHelper(StringBuilder logger, ActionContextInfo context)
        {
            _logger = logger;
            Context = context;
        }


        public List<SignerData> GetSigners(SendEnvelopeConfig Configuration)
        {
            _logger.AppendLine("Downloading signers");
            var signers = new List<SignerData>();
            foreach (var row in Context.CurrentDocument.ItemsLists.GetByID(Configuration.RecipientsSelection.SignersList.ItemListId).Rows)
            {
                var signerName = WebCon.WorkFlow.SDK.Tools.Other.TextHelper.GetPairName(row.Cells.GetByID(Configuration.RecipientsSelection.SignersList.SignerNameColumnID).GetValue()?.ToString());
                var signerMail = row.Cells.GetByID(Configuration.RecipientsSelection.SignersList.SignerMailColumnID).GetValue().ToString();
                var signerPhoneNumber = row.Cells.GetByID(Configuration.RecipientsSelection.SignersList.SignerPhoneNumberColumnID).GetValue().ToString();
                signers.Add(new SignerData(signerName, signerMail, signerPhoneNumber));
            }

            return signers;
        }

        public List<SignerData> GetEmbededSigner(SendEnvelopeToEmbededSignConfig config)
        {
            return new List<SignerData>()
            {
                new SignerData(config.RecipientSelection.SignerName,
                config.RecipientSelection.SignerMail,
                config.RecipientSelection.SignerPhoneNumber)
            };
        }

        public List<AttachmentData> GetDocuments(SendEnvelopeConfigurationBase configuration)
        {
            var attachments = new List<AttachmentData>();
            if (configuration.AttachmentSelection.AttachmentsChoosingOption == AttachmentsChoosingOptions.Category)
            {
                _logger.AppendLine("Downloading attachments by category");

                foreach (var att in Context.CurrentDocument.Attachments)
                    if (IsValidCategory(att, configuration))
                        if (string.IsNullOrEmpty(configuration.AttachmentSelection.AttRegularExpression) || Regex.IsMatch(att.FileName, configuration.AttachmentSelection.AttRegularExpression))
                            attachments.Add(att);

                return attachments;
            }
            _logger.AppendLine("Downloading attachments by SQL query");
            var attIDs = WebCon.WorkFlow.SDK.Tools.Data.SqlExecutionHelper.GetDataTableForSqlCommand(configuration.AttachmentSelection.AttQuery, Context);
            foreach (DataRow row in attIDs.Rows)
            {
                var att = WebCon.WorkFlow.SDK.Documents.DocumentAttachmentsManager.GetAttachment(row.Field<int>(0));
                attachments.Add(att);
            }
            return attachments;
        }

        private bool IsValidCategory(AttachmentData att, SendEnvelopeConfigurationBase configuration)
        {
            bool isValid = false;
            switch (configuration.AttachmentSelection.CategorySelectionOptions)
            {
                case CategorySelectionOptions.None:
                    isValid = att.FileGroup == null;
                    break;

                case CategorySelectionOptions.All:
                    isValid = true;
                    break;

                default:
                    isValid = att?.FileGroup?.ID == configuration.AttachmentSelection.GroupID;
                    break;
            }
            return isValid;
        }
    }
}

public class SignerData
{
    public SignerData(string name, string mail, string phoneNumer)
    {
        Name = name;
        Mail = mail;
        PhoneNumber = phoneNumer;
    }

    public string Name { get; set; }
    public string Mail { get; set; }
    public string PhoneNumber { get; set; }
}

