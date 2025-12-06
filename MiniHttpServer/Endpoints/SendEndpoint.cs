using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Web;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class SendEndpoint : BaseEndpoint
    {
        [HttpPost]
        public async Task<string> SendEmail()
        {
            try
            {
                string requestBody;
                using (var reader = new StreamReader(Context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                Console.WriteLine("Получены данные: " + requestBody);
                
                // парсим form data формат: email=test@test.com
                string emailValue = "";
                var parts = requestBody.Split('&');
                foreach (var part in parts)
                {
                    if (part.StartsWith("email="))
                    {
                        emailValue = part.Substring(6); // убираем "email="
                        emailValue = System.Web.HttpUtility.UrlDecode(emailValue);
                        break;
                    }
                }
                
                Console.WriteLine("Email: " + emailValue);

                await EmailService.SendEmailAsync(emailValue, "Рассылка", "Вы подписались на спам рассылку. Поздравляем!", "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка в SendEmail: {ex.Message}");
            }
            
            // всегда редиректим на главную, даже если была ошибка
            try
            {
                Context.Response.Redirect("/travel");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка редиректа: {ex.Message}");
            }
            return "";
        }
    }
        
    public class Email
    {
        public string email { get; set; }
    }
}
