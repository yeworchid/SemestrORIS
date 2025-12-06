using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Helpers;
using MiniHttpServer.Repositories;
using MiniHttpServer.Framework.Settings;
using MyORMLibrary;
using System.Text.Json;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AdminEndpoint : BaseEndpoint
    {
        private readonly TourRepository _tourRepository;

        public AdminEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var orm = new ORMContext(settings.ConnectionString);
            _tourRepository = new TourRepository(orm);
        }

        // страница админки
        [HttpGet]
        public async Task<IResponseResult> AdminPage()
        {
            // проверяем залогинен ли юзер
            var user = SessionHelper.GetCurrentUser(Context);
            
            if (user == null)
            {
                Console.WriteLine("юзер не залогинен, редирект на логин");
                Context.Response.Redirect("/auth");
                return Json(new { redirect = true });
            }
            
            // проверяем что юзер админ
            if (user.Role != "admin")
            {
                Console.WriteLine($"юзер {user.Email} не админ, доступ запрещен");
                throw new UnauthorizedAccessException("Доступ запрещен. Только для администраторов.");
            }
            
            Console.WriteLine($"админ {user.Email} зашел в админку");
            
            // получаем все туры
            var tours = await _tourRepository.GetAllAsync();
            
            // показываем админку
            return Page("Templates/Pages/admin.thtml", new 
            { 
                user = user,
                tours = tours,
                message = $"Привет, {user.FullName ?? user.Email}!"
            });
        }

        // редактирование тура
        [HttpPost("edit")]
        public async Task<IResponseResult> EditTour()
        {
            try
            {
                // проверяем админа
                var user = SessionHelper.GetCurrentUser(Context);
                if (user == null || user.Role != "admin")
                {
                    return Json(new { success = false, message = "Доступ запрещен" });
                }

                // читаем данные
                string requestBody;
                using (var reader = new StreamReader(Context.Request.InputStream))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                var tourData = JsonSerializer.Deserialize<TourEditRequest>(requestBody);
                
                // обновляем через репозиторий
                await _tourRepository.UpdateBasicFieldsAsync(
                    tourData.id, 
                    tourData.title, 
                    tourData.shortDescription,
                    tourData.fullDescription,
                    tourData.basePrice
                );

                Console.WriteLine($"тур {tourData.id} обновлен");
                return Json(new { success = true, message = "Тур обновлен" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка при редактировании: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // для парсинга данных редактирования
    public class TourEditRequest
    {
        public int id { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
        public string fullDescription { get; set; }
        public decimal basePrice { get; set; }
    }
}
