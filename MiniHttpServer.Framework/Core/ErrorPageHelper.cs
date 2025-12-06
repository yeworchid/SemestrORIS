using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Core
{
    public static class ErrorPageHelper
    {
        public static void SendErrorPage(HttpListenerContext context)
        {
            try
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html; charset=utf-8";
                
                string errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Ошибка</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
        .error-container { background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); max-width: 600px; margin: 100px auto; text-align: center; }
        h1 { color: #d32f2f; margin-bottom: 20px; }
        .message { background: #ffebee; padding: 20px; border-left: 4px solid #d32f2f; margin: 20px 0; }
        .home-link { display: inline-block; margin-top: 20px; padding: 12px 24px; background: #1976d2; color: white; text-decoration: none; border-radius: 4px; font-weight: bold; }
        .home-link:hover { background: #1565c0; }
    </style>
</head>
<body>
    <div class='error-container'>
        <h1>Произошла ошибка</h1>
        <div class='message'>
            <p>К сожалению, произошла ошибка при обработке вашего запроса.</p>
        </div>
        <p>Не переживайте, сервер продолжает работать. Вы можете вернуться на главную страницу и продолжить работу.</p>
        <a href='/' class='home-link'>Вернуться на главную</a>
    </div>
</body>
</html>";
                
                byte[] buffer = Encoding.UTF8.GetBytes(errorHtml);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();
                
                Console.WriteLine("[INFO] Отправлена страница ошибки");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Не удалось отправить страницу ошибки: {ex.Message}");
                try { context.Response.Close(); } catch { }
            }
        }
    }
}
