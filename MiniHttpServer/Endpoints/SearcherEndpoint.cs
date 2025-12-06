using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class SearcherEndpoint
    {
        // /searcher
        [HttpGet]
        public string Searcher()
        {
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: {settings.SearcherUri}/index.html");
            return settings.SearcherUri + "/index.html";
        }
    }
}