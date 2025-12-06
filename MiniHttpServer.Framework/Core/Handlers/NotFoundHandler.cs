using MiniHttpServer.Framework.Core.Abstracts;
using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Core.Handlers
{
    internal class NotFoundHandler : Handler
    {
        public override async void HandleRequest(HttpListenerContext context)
        {
            var response = context.Response;
            response.StatusCode = 404;
            response.ContentType = "text/html; charset=utf-8";

            string errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <title>404 - Страница не найдена</title>
</head>
<body>
    <h1>404 - Страница не найдена</h1>
    <p>Запрашиваемый ресурс не найден на сервере.</p>
</body>
</html>";

            byte[] buffer = Encoding.UTF8.GetBytes(errorHtml);
            response.ContentLength64 = buffer.Length;

            using var output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            await output.FlushAsync();

            Console.WriteLine($"404 ошибка: {context.Request.Url?.AbsolutePath}");
        }
    }
}