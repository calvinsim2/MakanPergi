using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Common.Helpers
{
    public class MyHttpUtility
    {
        public static HttpClient GetPortalHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (HttpRequestMessage request, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true;
            var client = new HttpClient(handler);
            //client.DefaultRequestHeaders.Add("SecretKey", secretKey);
            return client;
        }
    }
}
