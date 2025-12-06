using System.Net;

namespace MiniHttpServer.Framework.Core.HttpResponse;

public abstract class BaseEndpoint
{
    protected HttpListenerContext Context { get; private set; }

    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    // метод для возврата HTML страницы с данными через шаблонизатор
    protected IResponseResult Page(string pathTemplate, object data) => new PageResult(pathTemplate, data);
    
    // метод для возврата JSON ответа
    protected IResponseResult Json(object data) => new JsonResult(data);
}