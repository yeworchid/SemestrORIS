using System.Collections.Generic;
using Xunit;

namespace MyORMLibrary.Tests
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ORMContextTests
    {
        private string connectionString = "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=password";

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            var context = new ORMContext(connectionString);
            Assert.NotNull(context);
        }

        [Fact]
        public async Task Create_Test()
        {
            var context = new ORMContext(connectionString);
            var newUser = new User 
            { 
                Name = "TestUser", 
                Age = 25 
            };

            var result = await context.CreateAsync(newUser, "users");
            
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReadById_Test()
        {
            var context = new ORMContext(connectionString);
            var user = await context.ReadByIdAsync<User>(1, "users");
            
            Assert.NotNull(user);
        }

        [Fact]
        public async Task ReadAll_Test()
        {
            var context = new ORMContext(connectionString);
            var users = await context.ReadAllAsync<User>("users");
            
            Assert.NotNull(users);
        }

        [Fact]
        public async Task Read_WithoutFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var users = await context.ReadAsync<User>("users");
            
            Assert.NotNull(users);
        }

        [Fact]
        public async Task Read_WithSimpleFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@age", 25 } };
            var users = await context.ReadAsync<User>("users", "Age = @age", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public async Task Read_WithComplexFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@minAge", 18 }, { "@maxAge", 30 } };
            var users = await context.ReadAsync<User>("users", "Age >= @minAge AND Age <= @maxAge", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public async Task Read_WithLikeFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@name", "%John%" } };
            var users = await context.ReadAsync<User>("users", "Name LIKE @name", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public async Task Update_Test()
        {
            var context = new ORMContext(connectionString);
            var user = new User 
            { 
                Id = 1,
                Name = "Updated", 
                Age = 30 
            };

            await context.UpdateAsync(1, user, "users");
        }

        [Fact]
        public async Task Delete_Test()
        {
            var context = new ORMContext(connectionString);

            await context.DeleteAsync(999, "users");
        }
    }
}
