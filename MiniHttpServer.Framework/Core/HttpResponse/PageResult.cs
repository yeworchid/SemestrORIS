using System.Net;
using MiniTemplateEngine;

namespace MiniHttpServer.Framework.Core.HttpResponse;

internal class PageResult : IResponseResult
{
    private readonly string _pathTemplate;
    private readonly object _data;

    public PageResult(string pathTemplate, object data)
    {
        _pathTemplate = pathTemplate;
        _data = data;
    }

    public void Execute(HttpListenerContext context)
    {
        try
        {
            // создаем экземпляр шаблонизатора
            var renderer = new HtmlTemplateRenderer();
            
            // рендерим шаблон с данными
            string page = renderer.RenderFromFile(_pathTemplate, _data);

            // конвертируем в байты
            var buffer = System.Text.Encoding.UTF8.GetBytes(page);
            
            // устанавливаем тип контента
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            
            // отправляем данные
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
        catch
        {
            // если что-то пошло не так - просто закрываем соединение
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch { }
        }
    } 
}