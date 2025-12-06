using System;
using System.Collections.Generic;
using System.Reflection;
using Npgsql;

// класс для генерации SQL запросов
internal class SqlGenerator
{
    public static (string sql, NpgsqlCommand command) GenerateInsert<T>(T entity, string tableName, NpgsqlConnection connection) where T : class
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();
        
        var columns = new List<string>();
        var valueParams = new List<string>();
        var command = new NpgsqlCommand();
        command.Connection = connection;
        
        foreach (PropertyInfo property in properties)
        {
            if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                continue;
            
            columns.Add(property.Name);
            string paramName = $"@{property.Name}";
            valueParams.Add(paramName);
            
            object value = property.GetValue(entity);
            command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
        }
        
        string sql = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", valueParams)})";
        command.CommandText = sql;
        
        return (sql, command);
    }

    public static (string sql, NpgsqlCommand command) GenerateUpdate<T>(int id, T entity, string tableName, NpgsqlConnection connection) where T : class
    {
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();
        
        var setClauses = new List<string>();
        var command = new NpgsqlCommand();
        command.Connection = connection;
        
        foreach (PropertyInfo property in properties)
        {
            if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                continue;
            
            string paramName = $"@{property.Name}";
            setClauses.Add($"{property.Name} = {paramName}");
            
            object value = property.GetValue(entity);
            command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
        }
        
        string sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE Id = @id";
        command.CommandText = sql;
        command.Parameters.AddWithValue("@id", id);
        
        return (sql, command);
    }

    public static string GenerateSelect(string tableName, string whereClause = null)
    {
        string sql = $"SELECT * FROM {tableName}";
        if (!string.IsNullOrEmpty(whereClause))
        {
            sql += $" WHERE {whereClause}";
        }
        return sql;
    }

    public static string GenerateDelete(string tableName)
    {
        return $"DELETE FROM {tableName} WHERE Id = @id";
    }
}
