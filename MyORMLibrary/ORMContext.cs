using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using Npgsql;
 
public class ORMContext
{
    private readonly DatabaseExecutor _executor;
    private readonly string _connectionString;
 
    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
        _executor = new DatabaseExecutor(connectionString);
    }
    
    public string GetConnectionString()
    {
        return _connectionString;
    }
 
    public async Task<T> CreateAsync<T>(T entity, string tableName) where T : class
    {
        await _executor.ExecuteNonQueryAsync(async connection =>
        {
            var (sql, command) = SqlGenerator.GenerateInsert(entity, tableName, connection);
            await command.ExecuteNonQueryAsync();
        });
        
        return entity;
    }
 
    public async Task<T> ReadByIdAsync<T>(int id, string tableName) where T : class, new()
    {
        return await _executor.ExecuteSingleAsync(async connection =>
        {
            string sql = SqlGenerator.GenerateSelect(tableName, "Id = @id");
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
 
            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return EntityMapper.MapToObject<T>(reader);
                }
            }
            return null;
        });
    }
 
    public async Task<List<T>> ReadAllAsync<T>(string tableName) where T : class, new()
    {
        return await _executor.ExecuteListAsync(async connection =>
        {
            string sql = SqlGenerator.GenerateSelect(tableName);
            Console.WriteLine($"[ORM] выполняем SQL: {sql}");
            Console.WriteLine($"[ORM] строка подключения: {connection.ConnectionString}");
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
 
            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(EntityMapper.MapToObject<T>(reader));
                }
                Console.WriteLine($"[ORM] найдено {results.Count} records");
                return results;
            }
        });
    }
 
    public async Task UpdateAsync<T>(int id, T entity, string tableName) where T : class
    {
        await _executor.ExecuteNonQueryAsync(async connection =>
        {
            var (sql, command) = SqlGenerator.GenerateUpdate(id, entity, tableName, connection);
            await command.ExecuteNonQueryAsync();
        });
    }
 
    public async Task DeleteAsync(int id, string tableName)
    {
        await _executor.ExecuteNonQueryAsync(async connection =>
        {
            string sql = SqlGenerator.GenerateDelete(tableName);
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
 
            await command.ExecuteNonQueryAsync();
        });
    }

    // метод с фильтрацией
    public async Task<List<T>> ReadAsync<T>(string tableName, string whereClause = null, Dictionary<string, object> parameters = null) where T : class, new()
    {
        return await _executor.ExecuteListAsync(async connection =>
        {
            string sql = SqlGenerator.GenerateSelect(tableName, whereClause);
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }
 
            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(EntityMapper.MapToObject<T>(reader));
                }
                return results;
            }
        });
    }

    // FirstOrDefault с Expression
    public async Task<T> FirstOrDefaultAsync<T>(string tableName, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return await _executor.ExecuteSingleAsync(async connection =>
        {
            // парсим expression и генерируем SQL
            var (whereClause, parameters) = ExpressionParser.Parse(predicate);
            
            string sql = $"{SqlGenerator.GenerateSelect(tableName, whereClause)} LIMIT 1";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            // добавляем параметры
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
 
            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return EntityMapper.MapToObject<T>(reader);
                }
            }
            return null;
        });
    }

    // Where с Expression
    public async Task<List<T>> WhereAsync<T>(string tableName, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        return await _executor.ExecuteListAsync(async connection =>
        {
            // парсим expression и генерируем SQL
            var (whereClause, parameters) = ExpressionParser.Parse(predicate);
            
            string sql = SqlGenerator.GenerateSelect(tableName, whereClause);
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            
            // добавляем параметры
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value);
            }
 
            await using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                var results = new List<T>();
                while (await reader.ReadAsync())
                {
                    results.Add(EntityMapper.MapToObject<T>(reader));
                }
                return results;
            }
        });
    }
}