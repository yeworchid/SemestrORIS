using System.Threading.Tasks;
using System.Text.Json;
using System.Net;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Services;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Repositories;
using MiniHttpServer.Models;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class TravelEndpoint : BaseEndpoint
    {
        private readonly TourRepository _tourRepository;

        public TravelEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var orm = new ORMContext(settings.ConnectionString);
            _tourRepository = new TourRepository(orm);
        }

        [HttpGet]
        public async Task<IResponseResult> CatalogPage()
        {
            // проверяем есть ли id в query string - если есть, показываем детальную страницу
            var idStr = Context.Request.QueryString["id"];
            
            if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out int tourId))
            {
                // это запрос детальной страницы
                Console.WriteLine($"запрос детальной страницы для тура id={tourId}");
                
                var tour = await _tourRepository.GetByIdAsync(tourId);
                
                if (tour == null)
                {
                    Console.WriteLine($"[WARN] тур с id={tourId} не найден");
                    var emptyTours = await _tourRepository.GetAllAsync();
                    return Page("Templates/Travel/index.thtml", new { tours = emptyTours });
                }

                return Page("Templates/Travel/detailed.thtml", new { tour });
            }
            
            // проверяем параметры фильтрации
            var search = Context.Request.QueryString["search"];
            var country = Context.Request.QueryString["country"];
            var minPriceStr = Context.Request.QueryString["min_price"];
            var maxPriceStr = Context.Request.QueryString["max_price"];
            var dateFromStr = Context.Request.QueryString["date_from"];
            var dateToStr = Context.Request.QueryString["date_to"];
            
            // если есть хоть один параметр фильтрации - используем фильтрацию
            bool hasFilters = !string.IsNullOrEmpty(search) || 
                            !string.IsNullOrEmpty(country) || 
                            !string.IsNullOrEmpty(minPriceStr) || 
                            !string.IsNullOrEmpty(maxPriceStr) ||
                            !string.IsNullOrEmpty(dateFromStr) ||
                            !string.IsNullOrEmpty(dateToStr);
            
            List<Tour> tours;
            
            if (hasFilters)
            {
                // парсим цены
                decimal? minPrice = null;
                decimal? maxPrice = null;
                
                if (!string.IsNullOrEmpty(minPriceStr) && decimal.TryParse(minPriceStr, out decimal min))
                {
                    minPrice = min;
                }
                
                if (!string.IsNullOrEmpty(maxPriceStr) && decimal.TryParse(maxPriceStr, out decimal max))
                {
                    maxPrice = max;
                }
                
                // парсим даты с валидацией регуляркой
                DateTime? dateFrom = null;
                DateTime? dateTo = null;
                var dateRegex = new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2}$");
                
                if (!string.IsNullOrEmpty(dateFromStr))
                {
                    if (dateRegex.IsMatch(dateFromStr) && DateTime.TryParse(dateFromStr, out DateTime parsedFrom))
                    {
                        dateFrom = parsedFrom;
                    }
                    else
                    {
                        Console.WriteLine($"невалидная дата от: {dateFromStr}");
                    }
                }
                
                if (!string.IsNullOrEmpty(dateToStr))
                {
                    if (dateRegex.IsMatch(dateToStr) && DateTime.TryParse(dateToStr, out DateTime parsedTo))
                    {
                        dateTo = parsedTo;
                    }
                    else
                    {
                        Console.WriteLine($"невалидная дата до: {dateToStr}");
                    }
                }
                
                Console.WriteLine($"фильтрация: search={search}, country={country}, minPrice={minPrice}, maxPrice={maxPrice}, dateFrom={dateFrom}, dateTo={dateTo}");
                tours = await _tourRepository.GetFilteredAsync(search, country, minPrice, maxPrice, dateFrom, dateTo);
            }
            else
            {
                // без фильтров - показываем все
                tours = await _tourRepository.GetAllAsync();
            }

            Console.WriteLine($"отдаем страничку: Travel, туров найдено: {tours.Count}");
            
            if (tours.Count > 0)
            {
                var firstTour = tours[0];
                Console.WriteLine($"Первый тур: Id={firstTour.Id}, Title={firstTour.Title}, Price={firstTour.BasePrice}, Desc={firstTour.ShortDescription}");
            }
            
            return Page("Templates/Travel/index.thtml", new { tours });
        }
    }
}
