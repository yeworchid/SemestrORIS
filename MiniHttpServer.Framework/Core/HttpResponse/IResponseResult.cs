using System.Net;

namespace MiniHttpServer.Framework.Core.HttpResponse;

public interface IResponseResult
{
    void Execute(HttpListenerContext context);
}