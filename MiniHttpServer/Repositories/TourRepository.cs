using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;
using Npgsql;

namespace MiniHttpServer.Repositories
{
    public class TourRepository : IRepository<Tour>
    {
        private readonly ORMContext _context;
        private readonly string _connectionString;
        private const string TableName = "tours";

        public TourRepository(ORMContext context)
        {
            _context = context;
            _connectionString = context.GetConnectionString();
        }

        public async Task<List<Tour>> GetAllAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
            
            var tours = new List<Tour>();
            
            // загружаем туры
            var sql = @"
                SELECT 
                    t.id,
                    t.title,
                    t.short_description,
                    t.full_description,
                    t.additional_description,
                    t.included_in_price,
                    t.not_included_in_price,
                    t.base_price,
                    t.duration,
                    t.tour_type,
                    c.name as city_name,
                    c.country_id,
                    COALESCE(ti.image_url, 'https://www.kompass-komfort.de/media/cache/9/7/2/c/3/972c3df2b7ee901b7f7d0fc06f29b4108ee57350.jpeg') as cover_image_url
                FROM tours t
                LEFT JOIN cities c ON t.city_id = c.id
                LEFT JOIN tour_images ti ON t.id = ti.tour_id AND ti.is_cover = true
                ORDER BY t.id";
            
            await using (var command = new NpgsqlCommand(sql, connection))
            {
                await using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var tour = new Tour
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ShortDescription = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        FullDescription = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        AdditionalDescription = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        IncludedInPrice = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        NotIncludedInPrice = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        BasePrice = reader.GetDecimal(7),
                        Duration = reader.GetInt32(8),
                        TourType = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        CityName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        CountryId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        CoverImageUrl = reader.IsDBNull(12) ? "" : reader.GetString(12)
                    };
                    
                    tours.Add(tour);
                }
            } // reader закрывается здесь
            
            // теперь загружаем даты для всех туров
            await LoadTourDatesAsync(tours, connection);
            await LoadTourImagesAsync(tours, connection);
            
            Console.WriteLine($"[TourRepository] загружено {tours.Count} туров");
            return tours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в GetAllAsync: {ex.Message}");
                return new List<Tour>(); // возвращаем пустой список вместо падения
            }
        }
        
        private async Task LoadTourDatesAsync(List<Tour> tours, NpgsqlConnection connection)
        {
            if (tours.Count == 0) return;
            
            var tourIds = string.Join(",", tours.Select(t => t.Id));
            var sql = $@"
                SELECT 
                    id,
                    tour_id,
                    departure_date,
                    return_date,
                    available_seats,
                    status
                FROM tour_dates
                WHERE tour_id IN ({tourIds})
                ORDER BY tour_id, departure_date";
            
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            
            var datesByTourId = new Dictionary<int, List<TourDate>>();
            
            while (await reader.ReadAsync())
            {
                var tourDate = new TourDate
                {
                    Id = reader.GetInt32(0),
                    TourId = reader.GetInt32(1),
                    DepartureDate = reader.GetDateTime(2),
                    ReturnDate = reader.GetDateTime(3),
                    AvailableSeats = reader.GetInt32(4),
                    Status = reader.GetString(5)
                };
                
                if (!datesByTourId.ContainsKey(tourDate.TourId))
                {
                    datesByTourId[tourDate.TourId] = new List<TourDate>();
                }
                datesByTourId[tourDate.TourId].Add(tourDate);
            }
            
            // присваиваем даты турам
            foreach (var tour in tours)
            {
                if (datesByTourId.ContainsKey(tour.Id))
                {
                    tour.Dates = datesByTourId[tour.Id];
                }
            }
            
            Console.WriteLine($"[TourRepository] загружено дат для туров");
        }
        
        private async Task LoadTourImagesAsync(List<Tour> tours, NpgsqlConnection connection)
        {
            if (tours.Count == 0) return;
            
            var tourIds = string.Join(",", tours.Select(t => t.Id));
            var sql = $@"
                SELECT 
                    id,
                    tour_id,
                    image_url,
                    is_cover
                FROM tour_images
                WHERE tour_id IN ({tourIds})
                ORDER BY tour_id, is_cover DESC, id";
            
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            
            var imagesByTourId = new Dictionary<int, List<TourImage>>();
            
            while (await reader.ReadAsync())
            {
                var tourImage = new TourImage
                {
                    Id = reader.GetInt32(0),
                    TourId = reader.GetInt32(1),
                    ImageUrl = reader.GetString(2),
                    IsCover = reader.GetBoolean(3)
                };
                
                if (!imagesByTourId.ContainsKey(tourImage.TourId))
                {
                    imagesByTourId[tourImage.TourId] = new List<TourImage>();
                }
                imagesByTourId[tourImage.TourId].Add(tourImage);
            }
            
            // присваиваем картинки турам
            foreach (var tour in tours)
            {
                if (imagesByTourId.ContainsKey(tour.Id))
                {
                    tour.Images = imagesByTourId[tour.Id];
                }
            }
            
            Console.WriteLine($"[TourRepository] загружено картинок для туров");
        }

        public async Task<Tour?> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
            
            // загружаем тур
            var sql = @"
                SELECT 
                    t.id,
                    t.title,
                    t.short_description,
                    t.full_description,
                    t.additional_description,
                    t.included_in_price,
                    t.not_included_in_price,
                    t.base_price,
                    t.duration,
                    t.tour_type,
                    c.name as city_name,
                    c.country_id,
                    COALESCE(ti.image_url, 'https://www.kompass-komfort.de/media/cache/9/7/2/c/3/972c3df2b7ee901b7f7d0fc06f29b4108ee57350.jpeg') as cover_image_url
                FROM tours t
                LEFT JOIN cities c ON t.city_id = c.id
                LEFT JOIN tour_images ti ON t.id = ti.tour_id AND ti.is_cover = true
                WHERE t.id = @id";
            
            Tour? tour = null;
            
            await using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", id);
                await using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    tour = new Tour
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ShortDescription = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        FullDescription = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        AdditionalDescription = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        IncludedInPrice = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        NotIncludedInPrice = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        BasePrice = reader.GetDecimal(7),
                        Duration = reader.GetInt32(8),
                        TourType = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        CityName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        CountryId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        CoverImageUrl = reader.IsDBNull(12) ? "" : reader.GetString(12)
                    };
                }
            }
            
            if (tour != null)
            {
                // загружаем даты для тура
                var tours = new List<Tour> { tour };
                await LoadTourDatesAsync(tours, connection);
                await LoadTourImagesAsync(tours, connection);
                
                Console.WriteLine($"[TourRepository] загружен тур id={id}, дат: {tour.Dates.Count}, картинок: {tour.Images.Count}");
            }
            
            return tour;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в GetByIdAsync: {ex.Message}");
                return null; // возвращаем null вместо падения
            }
        }

        public async Task<Tour> CreateAsync(Tour entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(Tour entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }

        // обновление только основных полей тура
        public async Task UpdateBasicFieldsAsync(int id, string title, string shortDescription, string fullDescription, decimal basePrice)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var sql = @"UPDATE tours 
                       SET title = @title, 
                           short_description = @desc,
                           full_description = @fullDesc,
                           base_price = @price 
                       WHERE id = @id";
            
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc", shortDescription);
            cmd.Parameters.AddWithValue("@fullDesc", fullDescription);
            cmd.Parameters.AddWithValue("@price", basePrice);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        // метод для фильтрации туров
        public async Task<List<Tour>> GetFilteredAsync(string? search = null, string? country = null, decimal? minPrice = null, decimal? maxPrice = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
            
            var tours = new List<Tour>();
            
            // строим SQL запрос с фильтрами
            var sql = @"
                SELECT DISTINCT
                    t.id,
                    t.title,
                    t.short_description,
                    t.full_description,
                    t.additional_description,
                    t.included_in_price,
                    t.not_included_in_price,
                    t.base_price,
                    t.duration,
                    t.tour_type,
                    c.name as city_name,
                    c.country_id,
                    COALESCE(ti.image_url, 'https://www.kompass-komfort.de/media/cache/9/7/2/c/3/972c3df2b7ee901b7f7d0fc06f29b4108ee57350.jpeg') as cover_image_url
                FROM tours t
                LEFT JOIN cities c ON t.city_id = c.id
                LEFT JOIN tour_images ti ON t.id = ti.tour_id AND ti.is_cover = true";
            
            // если фильтруем по датам - нужен JOIN с tour_dates
            if (dateFrom.HasValue || dateTo.HasValue)
            {
                sql += " INNER JOIN tour_dates td ON t.id = td.tour_id";
            }
            
            sql += " WHERE 1=1";
            
            // добавляем условия фильтрации
            if (!string.IsNullOrEmpty(search))
            {
                sql += " AND LOWER(t.title) LIKE LOWER(@search)";
            }
            
            if (!string.IsNullOrEmpty(country))
            {
                sql += " AND c.country_id = @countryId";
            }
            
            if (minPrice.HasValue)
            {
                sql += " AND t.base_price >= @minPrice";
            }
            
            if (maxPrice.HasValue)
            {
                sql += " AND t.base_price <= @maxPrice";
            }
            
            if (dateFrom.HasValue)
            {
                sql += " AND td.departure_date >= @dateFrom";
            }
            
            if (dateTo.HasValue)
            {
                sql += " AND td.departure_date <= @dateTo";
            }
            
            sql += " ORDER BY t.id";
            
            await using (var command = new NpgsqlCommand(sql, connection))
            {
                // добавляем параметры
                if (!string.IsNullOrEmpty(search))
                {
                    command.Parameters.AddWithValue("@search", "%" + search + "%");
                }
                
                if (!string.IsNullOrEmpty(country) && int.TryParse(country, out int countryId))
                {
                    command.Parameters.AddWithValue("@countryId", countryId);
                }
                
                if (minPrice.HasValue)
                {
                    command.Parameters.AddWithValue("@minPrice", minPrice.Value);
                }
                
                if (maxPrice.HasValue)
                {
                    command.Parameters.AddWithValue("@maxPrice", maxPrice.Value);
                }
                
                if (dateFrom.HasValue)
                {
                    command.Parameters.AddWithValue("@dateFrom", dateFrom.Value);
                }
                
                if (dateTo.HasValue)
                {
                    command.Parameters.AddWithValue("@dateTo", dateTo.Value);
                }
                
                await using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var tour = new Tour
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        ShortDescription = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        FullDescription = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        AdditionalDescription = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        IncludedInPrice = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        NotIncludedInPrice = reader.IsDBNull(6) ? "" : reader.GetString(6),
                        BasePrice = reader.GetDecimal(7),
                        Duration = reader.GetInt32(8),
                        TourType = reader.IsDBNull(9) ? "" : reader.GetString(9),
                        CityName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        CountryId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                        CoverImageUrl = reader.IsDBNull(12) ? "" : reader.GetString(12)
                    };
                    
                    tours.Add(tour);
                }
            }
            
            // загружаем даты и картинки
            await LoadTourDatesAsync(tours, connection);
            await LoadTourImagesAsync(tours, connection);
            
            Console.WriteLine($"[TourRepository] отфильтровано {tours.Count} туров");
            return tours;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в GetFilteredAsync: {ex.Message}");
                return new List<Tour>(); // возвращаем пустой список вместо падения
            }
        }
    }
}
