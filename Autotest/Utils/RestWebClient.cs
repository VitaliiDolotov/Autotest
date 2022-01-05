using Autotest.Providers;
using RestSharp;

namespace Autotest.Utils
{
    public class RestWebClient
    {
        public RestWebClient()
        {
            Mofw = new RestClient(UrlProvider.RestClientBaseUrl);
        }

        public RestClient Mofw { get;}
    }
}