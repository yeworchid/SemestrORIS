using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class GptEndpoint
    {
        [HttpGet]
        public string ChatGPT()
        {
            var settings = Singleton.GetInstance().Settings;
            return settings.ChatGPTUri + "/index.html";
        }
    }
}