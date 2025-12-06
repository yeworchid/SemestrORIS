using System;
using System.Reflection;
using Npgsql;

// класс для маппинга данных из БД в объекты
internal class EntityMapper
{
    public static T MapToObject<T>(NpgsqlDataReader reader) where T : class, new()
    {
        T obj = new T();
        Type type = typeof(T);
        PropertyInfo[] properties = type.GetProperties();
        
        foreach (PropertyInfo property in properties)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                
                // пробуем прямое совпадение с игнорированием регистра
                if (property.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    object value = reader.GetValue(i);
                    
                    if (value != DBNull.Value)
                    {
                        property.SetValue(obj, value);
                    }
                    break;
                }
                
                // пробуем конвертировать snake_case в PascalCase
                string pascalCaseColumnName = ConvertSnakeCaseToPascalCase(columnName);
                if (property.Name.Equals(pascalCaseColumnName, StringComparison.Ordinal))
                {
                    object value = reader.GetValue(i);
                    
                    if (value != DBNull.Value)
                    {
                        property.SetValue(obj, value);
                    }
                    break;
                }
            }
        }
        
        return obj;
    }
    
    // конвертирует snake_case в PascalCase
    // например: short_description -> ShortDescription
    private static string ConvertSnakeCaseToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;
            
        var parts = snakeCase.Split('_');
        var result = string.Empty;
        
        foreach (var part in parts)
        {
            if (part.Length > 0)
            {
                result += char.ToUpper(part[0]) + part.Substring(1).ToLower();
            }
        }
        
        return result;
    }
}
