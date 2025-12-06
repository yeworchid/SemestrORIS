using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class TourImageRepository : IRepository<TourImage>
    {
        private readonly ORMContext _context;
        private const string TableName = "tour_images";

        public TourImageRepository(ORMContext context)
        {
            _context = context;
        }

        public async Task<List<TourImage>> GetAllAsync()
        {
            return await _context.ReadAllAsync<TourImage>(TableName);
        }

        public async Task<TourImage?> GetByIdAsync(int id)
        {
            return await _context.ReadByIdAsync<TourImage>(id, TableName);
        }

        public async Task<TourImage> CreateAsync(TourImage entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(TourImage entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }
    }
}
