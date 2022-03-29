using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using System;
using System.Linq;
using System.Text;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using WebCon.BpsExt.Signing.DocuSign.CustomActions.Configuration;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    internal class ApiHelperBase
    {
        private const int TOKEN_REPLACEMENT_IN_SECONDS = 10 * 60;

        private string AccessToken { get; set; }
        private DateTime expiresAt;
        private Account Account { get; set; }
        private readonly ApiConfigurationBase Config;
        private readonly StringBuilder _logger;

        protected ApiClient ApiClient { get; private set; }

        protected string AccountID
        {
            get { return Account.AccountId; }
        }

        public ApiHelperBase(ApiClient client, ApiConfigurationBase config, StringBuilder logger)
        {
            ApiClient = client;
            Config = config;
            _logger = logger;
        }

        public void CheckToken()
        {
            if (AccessToken == null
                || (DateTime.Now  > expiresAt))
            {
                _logger.AppendLine("Obtaining a new access token...");
                UpdateToken();
            }
        }

        private void UpdateToken()
        {
            OAuth.OAuthToken authToken = ApiClient.RequestJWTUserToken(Config.ClientID,
                            Config.ImpersonatedUserGuid,
                            Config.AuthServer,
                            Encoding.UTF8.GetBytes(Config.PrivateKey),
                            1);

            AccessToken = authToken.access_token;

            if (Account == null)
                Account = GetAccountInfo(authToken);

            ApiClient = new ApiClient(Account.BaseUri + "/restapi");
            ApiClient.Configuration.DefaultHeader.Add("Authorization", $"Bearer {AccessToken}");
            expiresAt = DateTime.Now.AddSeconds(authToken.expires_in.Value);
        }

        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            ApiClient.SetOAuthBasePath(Config.AuthServer);
            OAuth.UserInfo userInfo = ApiClient.GetUserInfo(authToken.access_token);
            var accounts = userInfo.Accounts;
            return accounts.FirstOrDefault(a => a.IsDefault == "true");
        }
    }
}
