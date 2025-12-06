using System.Net;
using System.Text.Json;

namespace MiniHttpServer.Framework.Core.HttpResponse;

// класс для возврата JSON ответов из endpoint'ов
internal class JsonResult : IResponseResult
{
    private readonly object _data;

    public JsonResult(object data)
    {
        _data = data;
    }

    public void Execute(HttpListenerContext context)
    {
        // сериализуем объект в JSON
        string json = JsonSerializer.Serialize(_data);
        
        // конвертируем в байты
        var buffer = System.Text.Encoding.UTF8.GetBytes(json);
        
        // устанавливаем тип контента
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = buffer.Length;
        
        // отправляем данные
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }
}
