using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration
{
    public class SendDocumentOutputParameters
    {
        [ConfigEditableFormFieldID(DisplayName = "Copy Envelope ID to field", IsRequired = true,
Description = "Specify a text field on the form where envelope ID will be saved")]
        public int EnvelopeFieldId { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Copy sent document ID to field", IsRequired = true,
            Description = "Specify a text field on the form where external documents ID will be saved")]
        public int TechnicalFieldID { get; set; }

    }
}
