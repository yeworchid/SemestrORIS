using System.Net;
using System.Net.Mail;
using System.Text;
using MiniHttpServer.Framework.Settings;

namespace MiniHttpServer.Services
{
    public static class EmailService
    {

        public static async Task SendEmailAsync(string to, string subject, string message, string zipRelativePath)
        {
            var settings = Singleton.GetInstance().Settings;

            string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..")); 
            string zipFilePath = Path.Combine(projectRoot, zipRelativePath);
            
            Console.WriteLine("=== отправка письма ===");
            Console.WriteLine($"кому отправляем: {to}");
            Console.WriteLine($"тема письма: {subject}");
            Console.WriteLine($"что пишем: {message}");
            
            try
            {
                Console.WriteLine("подключаемся к гугл серверу...");
                using (var smtpClient = new SmtpClient(settings.SmtpServer, settings.SmtpPort))
                {
                    Console.WriteLine($"подключение к: {settings.SmtpServer}:{settings.SmtpPort}");
                    
                    smtpClient.Credentials = new NetworkCredential(settings.FromEmail, settings.AppPassword);
                    smtpClient.EnableSsl = true;

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(settings.FromEmail);
                        mailMessage.To.Add(to);
                        mailMessage.Subject = subject;
                        mailMessage.Body = message;
                        mailMessage.IsBodyHtml = false;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;

                        // прикладываем zip (почта блокирует архив)
                        // if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
                        // {
                        //     var attachment = new Attachment(zipFilePath);
                        //     mailMessage.Attachments.Add(attachment);
                        // }

                        Console.WriteLine("все готово, шлем письмо...");
                        await smtpClient.SendMailAsync(mailMessage);
                        Console.WriteLine("✅ письмо ушло!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ не получилось отправить: {ex.Message}");
                throw;
            }
        }
    }
}