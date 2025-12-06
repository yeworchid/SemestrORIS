using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class CountryRepository : IRepository<Country>
    {
        private readonly ORMContext _context;
        private const string TableName = "countries";

        public CountryRepository(ORMContext context)
        {
            _context = context;
        }

        public async Task<List<Country>> GetAllAsync()
        {
            return await _context.ReadAllAsync<Country>(TableName);
        }

        public async Task<Country?> GetByIdAsync(int id)
        {
            return await _context.ReadByIdAsync<Country>(id, TableName);
        }

        public async Task<Country> CreateAsync(Country entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(Country entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }
    }
}
