using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class TourDateRepository : IRepository<TourDate>
    {
        private readonly ORMContext _context;
        private const string TableName = "tour_dates";

        public TourDateRepository(ORMContext context)
        {
            _context = context;
        }

        public async Task<List<TourDate>> GetAllAsync()
        {
            return await _context.ReadAllAsync<TourDate>(TableName);
        }

        public async Task<TourDate?> GetByIdAsync(int id)
        {
            return await _context.ReadByIdAsync<TourDate>(id, TableName);
        }

        public async Task<TourDate> CreateAsync(TourDate entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(TourDate entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }
    }
}
