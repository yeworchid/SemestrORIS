using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

// класс для выполнения запросов к БД
internal class DatabaseExecutor
{
    private readonly string _connectionString;

    public DatabaseExecutor(string connectionString)
    {
        _connectionString = connectionString;
    }

    // выполнение запроса без возврата данных
    public async Task ExecuteNonQueryAsync(Func<NpgsqlConnection, Task> action)
    {
        await using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await action(connection);
        }
    }

    // выполнение запроса с возвратом одного объекта
    public async Task<T> ExecuteSingleAsync<T>(Func<NpgsqlConnection, Task<T>> action)
    {
        await using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await action(connection);
        }
    }

    // выполнение запроса с возвратом списка объектов
    public async Task<List<T>> ExecuteListAsync<T>(Func<NpgsqlConnection, Task<List<T>>> action)
    {
        await using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await action(connection);
        }
    }
}
