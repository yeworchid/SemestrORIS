using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class CityRepository : IRepository<City>
    {
        private readonly ORMContext _context;
        private const string TableName = "cities";

        public CityRepository(ORMContext context)
        {
            _context = context;
        }

        public async Task<List<City>> GetAllAsync()
        {
            return await _context.ReadAllAsync<City>(TableName);
        }

        public async Task<City?> GetByIdAsync(int id)
        {
            return await _context.ReadByIdAsync<City>(id, TableName);
        }

        public async Task<City> CreateAsync(City entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(City entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }
    }
}
