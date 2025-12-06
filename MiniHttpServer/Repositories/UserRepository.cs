using MiniHttpServer.Interfaces;
using MiniHttpServer.Models;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly ORMContext _context;
        private const string TableName = "users";

        public UserRepository(ORMContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.ReadAllAsync<User>(TableName);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.ReadByIdAsync<User>(id, TableName);
        }

        public async Task<User> CreateAsync(User entity)
        {
            return await _context.CreateAsync(entity, TableName);
        }

        public async Task UpdateAsync(User entity)
        {
            await _context.UpdateAsync(entity.Id, entity, TableName);
        }

        public async Task DeleteAsync(int id)
        {
            await _context.DeleteAsync(id, TableName);
        }

        // метод для поиска юзера по email
        public async Task<User?> GetByEmailAsync(string email)
        {
            var users = await _context.ReadAllAsync<User>(TableName);
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }
}