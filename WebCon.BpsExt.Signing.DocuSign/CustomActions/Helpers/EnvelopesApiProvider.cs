using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using System.Linq;
using System.Text;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using WebCon.WorkFlow.SDK.Tools.Data.Model;
using DocuSign.eSign.Api;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    internal class EnvelopesApiProvider
    {
        private string AccessToken { get; set; }
        private Account Account { get; set; }
        private readonly WebServiceConnection _connection;
        protected EnvelopesApi Client { get; private set; }
        private DocuSignClient _docusignClient;
        private ConnectionsHelper _connectionHelper;

        public EnvelopesApiProvider(WebServiceConnection connection, ConnectionsHelper connectionHelper)
        {
            _connection = connection;
            _connectionHelper = connectionHelper;
        }


        public (EnvelopesApi client, string accountId) CreateClient(bool useProxy)
        {
            _docusignClient = new DocuSignClient(_connection.AuthorizationServiceUrl, ProxyProvider.TryGetProxy(_connection.AuthorizationServiceUrl, useProxy, _connectionHelper));
            GetToken(useProxy);
            return (new EnvelopesApi(_docusignClient), Account.AccountId);
        }

        private void GetToken(bool useProxy)
        {
            OAuth.OAuthToken authToken = _docusignClient.RequestJWTUserToken(_connection.ClientID,
                            _connection.WebServiceUser,
                            _connection.AuthorizationServiceUrl,
                            Encoding.UTF8.GetBytes(_connection.ClientSecret),
                            1);

            AccessToken = authToken.access_token;

            if (Account == null)
                Account = GetAccountInfo(authToken);

            var url = Account.BaseUri + "/restapi";
            _docusignClient = new DocuSignClient(url, ProxyProvider.TryGetProxy(_connection.AuthorizationServiceUrl, useProxy, _connectionHelper));
            _docusignClient.Configuration.DefaultHeader.Add("Authorization", $"Bearer {AccessToken}");
            Client = new EnvelopesApi(_docusignClient);
        }

        private Account GetAccountInfo(OAuth.OAuthToken authToken)
        {
            _docusignClient.SetOAuthBasePath(_connection.AuthorizationServiceUrl);
            OAuth.UserInfo userInfo = _docusignClient.GetUserInfo(authToken.access_token);
            var accounts = userInfo.Accounts;
            return accounts.FirstOrDefault(a => a.IsDefault == "true");
        }
    }
}
