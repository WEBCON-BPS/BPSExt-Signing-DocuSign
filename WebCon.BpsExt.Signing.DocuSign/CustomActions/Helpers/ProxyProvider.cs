using System.Net;
using WebCon.WorkFlow.SDK.Tools.Data;

namespace WebCon.BpsExt.Signing.DocuSign.CustomActions.Helpers
{
    public static class ProxyProvider
    {
        public static IWebProxy TryGetProxy(string url, bool useProxy, ConnectionsHelper helper)
        {
            if (!useProxy)
                return null;

            return helper.GetProxy(url);
        }
    }
}
