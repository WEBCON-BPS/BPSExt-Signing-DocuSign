﻿using DocuSign.eSign.Client;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers;
using System;
using System.Text;
using WebCon.WorkFlow.SDK.ActionPlugins;
using WebCon.WorkFlow.SDK.ActionPlugins.Model;
using System.Net;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.SendReminder
{
    public class SendReminder : CustomAction<SendReminderConfig>
    {
        readonly StringBuilder _logger = new StringBuilder();
        public override void Run(RunCustomActionParams args)
        {
            try
            {
				var apiClient = new ApiClient(ApiClient.Production_REST_BasePath, ConnectionsHelper.GetProxy(ApiClient.Production_REST_BasePath) as WebProxy);
				var envelopeId = args.Context.CurrentDocument.Fields.GetByID(Configuration.EnvelopeSettings.EnvelopeGUIDFieldId).GetValue().ToString();
                _logger.AppendLine($"Sending reminder to recipients of envelope: {envelopeId}");
                var summary = new ApiHelper(apiClient, Configuration.ApiSettings, _logger).ResendEnvelope(envelopeId);
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
    }
}