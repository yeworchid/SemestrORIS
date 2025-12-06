using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Repositories;
using MiniHttpServer.Helpers;
using MyORMLibrary;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint : BaseEndpoint
    {
        private readonly UserRepository _userRepository;

        public AuthEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var orm = new ORMContext(settings.ConnectionString);
            _userRepository = new UserRepository(orm);
        }

        // показываем страницу логина
        [HttpGet]
        public IResponseResult LoginPage()
        {
            // проверяем залогинен ли юзер
            var user = SessionHelper.GetCurrentUser(Context);
            if (user != null)
            {
                Console.WriteLine($"юзер {user.Email} уже залогинен, редирект");
                Context.Response.Redirect("/");
                return Json(new { redirect = true });
            }
            
            var settings = Singleton.GetInstance().Settings;
            Console.WriteLine($"отдаем страничку: {settings.LoginUri}/login.html");
            Context.Response.Redirect(settings.LoginUri + "/login.html");
            return Json(new { redirect = true });
        }

        // обрабатываем логин
        [HttpPost]
        public async Task<IResponseResult> Login()
        {            
            try
            {
                // читаем данные из POST запроса (используем Context из BaseEndpoint)
                string requestBody;
                using (var reader = new StreamReader(Context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                
                var loginData = JsonSerializer.Deserialize<LoginRequest>(requestBody);
                
                // ищем юзера по email через репозиторий
                var user = await _userRepository.GetByEmailAsync(loginData.email);
                
                if (user == null)
                {
                    Console.WriteLine($"юзер с email {loginData.email} не найден");
                    return Page("Templates/Pages/login.thtml", new 
                    { 
                        success = false,
                        message = "Неверный email или пароль"
                    });
                }
                
                // хешируем введенный пароль через SHA256
                string hashedPassword = HashPassword(loginData.password);
                
                Console.WriteLine($"проверяем пароль для {loginData.email}");
                Console.WriteLine($"введенный хеш: {hashedPassword}");
                Console.WriteLine($"хеш из бд: {user.PasswordHash}");
                
                // сравниваем хеши
                bool passwordOk = hashedPassword == user.PasswordHash;
                
                if (!passwordOk)
                {
                    Console.WriteLine($"неверный пароль для {loginData.email}");
                    return Page("Templates/Pages/login.thtml", new 
                    { 
                        success = false,
                        message = "Неверный email или пароль"
                    });
                }
                
                // все ок, создаем сессию
                string token = SessionManager.CreateSession(user);
                SessionHelper.SetSessionCookie(Context, token);
                
                // шлем уведомление на почту
                string emailMessage = $@"
                Успешный вход в систему:
                Время: {DateTime.Now}
                Email: {loginData.email}
                Роль: {user.Role}
                ";
                
                Console.WriteLine($"шлем уведомление на {loginData.email}...");
                await EmailService.SendEmailAsync(loginData.email, "Успешный вход в систему", emailMessage, "");
                
                Console.WriteLine("логин прошел");
                
                // возвращаем страницу с результатом логина
                return Page("Templates/Pages/login.thtml", new 
                { 
                    success = true,
                    email = loginData.email,
                    role = user.Role,
                    message = "Вход выполнен успешно!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"что-то пошло не так с логином: {ex.Message}");
                
                // возвращаем страницу с ошибкой
                return Page("Templates/Pages/login.thtml", new 
                { 
                    success = false,
                    message = $"Ошибка: {ex.Message}"
                });
            }
        }

        // хешируем пароль через SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
    
    // чтобы парсить данные из формы логина
    public class LoginRequest
    {
        public string email { get; set; }
        public string password { get; set; }
    }
}
