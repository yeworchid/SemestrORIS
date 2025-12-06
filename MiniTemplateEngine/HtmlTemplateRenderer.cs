using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using MiniTemplateEngine.Interfaces;
using MiniTemplateEngine.Utils;

public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    /// <summary>
    /// Рендерит HTML шаблон с подстановкой переменных
    /// </summary>
    /// <param name="htmlTemplate">Шаблон с переменными ${name} и конструкциями $if/$foreach</param>
    /// <param name="dataModel">Объект с данными для подстановки</param>
    /// <returns>Готовый HTML с подставленными значениями</returns>
    public string RenderFromString(string htmlTemplate, object dataModel) // основной метод рендеринга
    {
        // сначала обрабатываем циклы и условия
        htmlTemplate = ProcessTemplate(htmlTemplate, dataModel);
        
        // потом простая подстановка переменных
        return Regex.Replace(htmlTemplate, @"\$\{([^}]+)\}", match => // ищем ${переменная}
        {
            var key = match.Groups[1].Value; // получаем имя переменной
            return Resolve(key, dataModel); // заменяем на значение
        });
    }

    private static string Resolve(string path, object model) // получает значение и превращает в строку
    {
        var value = ResolveValue(path, model); // получаем объект
        return value?.ToString() ?? ""; // превращаем в строку или пустую строку если null
    }

    private static object? ResolveValue(string path, object model) // получает значение по пути типа "user.name"
    {
        object? current = model; // начинаем с корневого объекта

        foreach (var part in path.Split('.')) // разбиваем путь по точкам
        {
            if (string.IsNullOrWhiteSpace(part) || current == null) // если пустая часть или null
                return null;

            var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance); // ищем свойство
            if (prop == null) // если свойство не найдено
                return null;

            current = prop.GetValue(current); // получаем значение свойства
        }

        return current; // возвращаем финальное значение
    }

    private string ProcessTemplate(string htmlTemplate, object dataModel) // обрабатывает шаблон построчно
    {
        var lines = htmlTemplate.Split('\n'); // разбиваем на строки
        var result = new StringBuilder(); // для сборки результата
        
        for (int i = 0; i < lines.Length; i++) // проходим по всем строкам
        {
            var line = lines[i]; // текущая строка
            
            // обработка условий $if
            if (line.Trim().StartsWith("$if("))
            {
                var (processedLines, newIndex) = ProcessIfBlock(lines, i, dataModel); // обрабатываем блок if
                result.Append(processedLines); // добавляем результат
                i = newIndex; // перескакиваем на конец блока
                continue; // переходим к следующей итерации
            }
            
            // обработка циклов $foreach
            if (line.Trim().StartsWith("$foreach("))
            {
                var (processedLines, newIndex) = ProcessForeachBlock(lines, i, dataModel); // обрабатываем блок foreach
                result.Append(processedLines); // добавляем результат
                i = newIndex; // перескакиваем на конец блока
                continue; // переходим к следующей итерации
            }
            
            // обычная строка
            result.AppendLine(line); // просто добавляем строку как есть
        }
        
        return result.ToString(); // возвращаем собранный результат
    }

    private (string result, int endIndex) ProcessIfBlock(string[] lines, int startIndex, object dataModel) // обрабатывает блок if-else
    {
        var result = new StringBuilder(); // для результата
        var line = lines[startIndex]; // строка с $if
        
        // парсим условие из $if(condition)
        var conditionMatch = Regex.Match(line, @"\$if\(([^)]+)\)"); // ищем условие в скобках
        if (!conditionMatch.Success) // если не нашли
            return ("", startIndex); // возвращаем пустоту
            
        var condition = conditionMatch.Groups[1].Value; // получаем условие
        bool conditionResult = CheckCondition(condition, dataModel); // проверяем условие
        
        var ifBody = new List<string>(); // строки для if
        var elseBody = new List<string>(); // строки для else
        bool inElse = false; // флаг
        int i = startIndex + 1; //
        int nestedLevel = 0; // уровень вложенности для правильного парсинга
        
        // собираем тело if и else с учетом вложенности
        while (i < lines.Length)
        {
            var currentLine = lines[i];
            var trimmed = currentLine.Trim();
            
            // проверяем на $else только на верхнем уровне
            if (trimmed == "$else" && nestedLevel == 0)
            {
                inElse = true;
                i++;
                continue;
            }
            
            // считаем уровень вложенности
            if (trimmed.StartsWith("$foreach(") || trimmed.StartsWith("$if("))
                nestedLevel++;
            else if (trimmed == "$endfor" || trimmed == "$endif")
            {
                if (nestedLevel == 0) // если это конец нашего блока
                    break;
                nestedLevel--;
            }
            
            if (inElse)
                elseBody.Add(currentLine);
            else
                ifBody.Add(currentLine);
                
            i++;
        }
        
        // выбираем какое тело обрабатывать
        var bodyToProcess = conditionResult ? ifBody : elseBody; // если условие true - if, иначе else
        
        // рекурсивно обрабатываем выбранное тело
        if (bodyToProcess.Count > 0)
        {
            var bodyText = string.Join("\n", bodyToProcess);
            var processedBody = ProcessTemplate(bodyText, dataModel);
            result.Append(processedBody);
        }
        
        return (result.ToString(), i);
    }

    private (string result, int endIndex) ProcessForeachBlock(string[] lines, int startIndex, object dataModel) // обрабатывает цикл foreach
    {
        var result = new StringBuilder();
        var line = lines[startIndex];
        
        // парсим foreach: $foreach(var item in object.Items)
        var foreachMatch = Regex.Match(line, @"\$foreach\(var\s+(\w+)\s+in\s+([^)]+)\)"); // ищем переменную и коллекцию
        if (!foreachMatch.Success)
            return ("", startIndex);
            
        var itemName = foreachMatch.Groups[1].Value; // имя переменной цикла (item)
        var collectionPath = foreachMatch.Groups[2].Value; // путь к коллекции (object.Items)
        
        // получаем коллекцию через рефлексию
        var collection = GetCollectionValue(collectionPath, dataModel);
        if (collection == null)
            return ("", startIndex);
        
        var loopBody = new List<string>(); // строки тела цикла
        int i = startIndex + 1;
        int nestedLevel = 0; // уровень вложенности
        
        // собираем тело цикла с учетом вложенности
        while (i < lines.Length)
        {
            var currentLine = lines[i];
            var trimmed = currentLine.Trim();
            
            // считаем уровень вложенности
            if (trimmed.StartsWith("$foreach(") || trimmed.StartsWith("$if("))
                nestedLevel++;
            else if (trimmed == "$endfor" || trimmed == "$endif")
            {
                if (nestedLevel == 0) // если это конец нашего цикла
                    break;
                nestedLevel--;
            }
            
            loopBody.Add(currentLine);
            i++;
        }
        
        // выполняем цикл для каждого элемента
        foreach (var item in collection)
        {
            foreach (var bodyLine in loopBody)
            {
                // заменяем переменную цикла на свойства текущего элемента
                var processedLine = bodyLine.Replace($"${{{itemName}.", "${"); // убираем имя переменной из пути
                
                // простая подстановка переменных для этой строки
                var finalLine = Regex.Replace(processedLine, @"\$\{([^}]+)\}", match =>
                {
                    var key = match.Groups[1].Value;
                    return Resolve(key, item);
                });
                
                result.AppendLine(finalLine);
            }
        }
        
        return (result.ToString(), i);
    }

    private System.Collections.IEnumerable? GetCollectionValue(string path, object model) // получает коллекцию по пути
    {
        try
        {
            object? current = model;
            
            foreach (var part in path.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(part) || current == null)
                    return null;

                var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                    return null;

                current = prop.GetValue(current);
            }
            
            return current as System.Collections.IEnumerable; // пытаемся привести к коллекции
        }
        catch
        {
            return null;
        }
    }

    private static bool CheckCondition(string expression, object model) // проверяет условие в if
    {
        expression = expression.Trim();

        // поддержка отрицания
        bool negate = false; // флаг отрицания
        if (expression.StartsWith("!"))
        {
            negate = true;
            expression = expression.Substring(1).Trim();
        }

        // поддержка простейших сравнений
        string[] ops = { "==", "!=", ">=", "<=", ">", "<" }; // список операторов сравнения
        foreach (var op in ops)
        {
            var parts = expression.Split(new string[] { op }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var leftValue = ResolveValue(parts[0].Trim(), model);
                var rightValue = parts[1].Trim().Trim('"', '\''); // убираем кавычки

                bool result = op switch
                {
                    "==" => leftValue?.ToString() == rightValue,
                    "!=" => leftValue?.ToString() != rightValue,
                    ">" => CompareValues(leftValue, rightValue) > 0,
                    "<" => CompareValues(leftValue, rightValue) < 0,
                    ">=" => CompareValues(leftValue, rightValue) >= 0,
                    "<=" => CompareValues(leftValue, rightValue) <= 0,
                    _ => false
                };

                return negate ? !result : result;
            }
        }

        // если нет оператора - проверяем логическое свойство
        var value = ResolveValue(expression, model);
        
        // если это булево значение
        if (value is bool boolValue)
        {
            return negate ? !boolValue : boolValue;
        }
            
        // если это строка с булевым значением
        if (value is string stringValue && bool.TryParse(stringValue, out var parsedBool))
        {
            return negate ? !parsedBool : parsedBool;
        }
            
        // если это число (0 = false, остальное = true)
        if (value is int intValue)
        {
            return negate ? intValue == 0 : intValue != 0; // 0 = false, остальное = true
        }

        // если объект не null = true
        bool objectResult = value != null;
        return negate ? !objectResult : objectResult;
    }

    private static int CompareValues(object? left, string right) // сравнивает два значения
    {
        try
        {
            if (left == null) return -1; // если левое null, то оно меньше
            
            if (left is IComparable comparable)
            {
                var convertedRight = Convert.ChangeType(right, left.GetType()); // приводим правое к типу левого
                return comparable.CompareTo(convertedRight);
            }
            
            return string.Compare(left.ToString(), right); // сравниваем как строки
        }
        catch
        {
            return 0; // считаем равными
        }
    }

    /// <summary>
    /// Читает шаблон из файла и рендерит его
    /// </summary>
    /// <param name="filePath">Путь к файлу с шаблоном</param>
    /// <param name="dataModel">Данные для подстановки</param>
    /// <returns>Обработанный HTML</returns>
    public string RenderFromFile(string filePath, object dataModel)
    {
        var template = File.ReadAllText(filePath);
        return RenderFromString(template, dataModel);
    }

    /// <summary>
    /// Рендерит шаблон из файла и сохраняет результат в другой файл
    /// </summary>
    /// <param name="inputFilePath">Путь к входному файлу с шаблоном</param>
    /// <param name="outputFilePath">Путь для сохранения результата</param>
    /// <param name="dataModel">Данные для подстановки</param>
    /// <returns>Обработанный HTML (тот же что записан в файл)</returns>
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        var result = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, result);
        return result;
    }
}