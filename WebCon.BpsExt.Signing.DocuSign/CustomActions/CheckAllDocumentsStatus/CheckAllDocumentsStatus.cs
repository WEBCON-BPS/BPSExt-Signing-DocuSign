using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using WebCon.WorkFlow.SDK.Documents.Model;
using System.Diagnostics;
using WebCon.WorkFlow.SDK.Documents;
using WebCon.WorkFlow.SDK.Tools.Data;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.CheckAllDocumentsStatus
{
    public class CheckAllDocumentsStatus : CustomAction<CheckAllDocumentsStatusConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override ActionTriggers AvailableActionTriggers => ActionTriggers.Recurrent;

        public override async Task RunWithoutDocumentContextAsync(RunCustomActionWithoutContextParams args)
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                var envelopesToCheck = await GetEnvelopesInfoAsync(args.Context);
                ConnectionsHelper connectionsHelper = new ConnectionsHelper(args.Context);
                var apiClient = new DocuSignClient(DocuSignClient.Production_REST_BasePath, connectionsHelper.GetProxy(DocuSignClient.Production_REST_BasePath));
                var allEnvelopes = new ApiHelper(apiClient, connectionsHelper, Configuration.ApiSettings, _logger).ListChanges(Configuration.EnvelopeLifetimeInDays);
                await DoActionsForEnvelopesAsync(allEnvelopes, envelopesToCheck, timer, args.Context);
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

        private async Task DoActionsForEnvelopesAsync(EnvelopesInformation allEnvelopes, List<EnvelopeInfo> envelopesToCheck, Stopwatch timer, ActionWithoutDocumentContext context)
        {
            var maxTime = TimeSpan.FromSeconds(Configuration.MaxExecutionTime);
            foreach (var item in envelopesToCheck)
            {
                if(timer.ElapsedMilliseconds > maxTime.TotalMilliseconds)
                {
                    _logger.AppendLine("Maximum execution time is exceeded, the action will be terminated");
                    return;
                }

                _logger.AppendLine($"Processing envelope with id: {item.EnvelopeGuid}");
                var env = allEnvelopes.Envelopes.FirstOrDefault(x => x.EnvelopeId == item.EnvelopeGuid);
                await ChoosePathForEnvelopeAsync(env, item, context);
            }
        }

        private async Task ChoosePathForEnvelopeAsync(Envelope env, EnvelopeInfo item, ActionWithoutDocumentContext context)
        {
            if(env == null)
            {
                _logger.AppendLine($"No envelope with id {item.EnvelopeGuid} was found in the downloaded envelopes. Such envelope does not exist or is older than 30 days.");
                return;
            }

            if (env.Status == "created" || env.Status == "sent" || env.Status == "delivered")
                return;

            var manager = new DocumentsManager(context);
            var doc = await manager.GetDocumentByIdAsync(item.WfdId, true);
            var parms = new MoveDocumentToNextStepParams(doc, Configuration.Workflow.ErrorPathId);
            parms.SkipPermissionsCheck = true;
            parms.ForceCheckout = true;
            if (env.Status == "completed")
                parms.PathID = Configuration.Workflow.SuccessPathId;

            await manager.MoveDocumentToNextStepAsync(parms);
        }

        private async Task<List<EnvelopeInfo>> GetEnvelopesInfoAsync(ActionWithoutDocumentContext context)
        {
            _logger.AppendLine("Executing SQL query");
            var sqlQuery = $"SELECT WFD_ID, {Configuration.Workflow.OperationFieldName} as 'GUID' from WFElements where WFD_STPID = {Configuration.Workflow.StepId}";
            var dt =  await new SqlExecutionHelper(context).GetDataTableForSqlCommandAsync(sqlQuery);
            return dt.AsEnumerable().Select(x =>
            new EnvelopeInfo(x.Field<int>("WFD_ID"), x.Field<string>("GUID"))).ToList();
        }
    }
    class EnvelopeInfo
    {
        public int WfdId { get; set; }
        public string EnvelopeGuid { get; set; }

        public EnvelopeInfo(int wfdId, string envelopeGuid)
        {
            WfdId = wfdId;
            EnvelopeGuid = envelopeGuid;
        }
    }
}