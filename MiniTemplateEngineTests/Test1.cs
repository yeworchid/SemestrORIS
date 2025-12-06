using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTemplateEngine.Interfaces;
using System;
using System.IO;

[TestClass]
public class MiniTemplateEngineTests
{
    private readonly IHtmlTemplateRenderer _renderer = new HtmlTemplateRenderer();

    // Тест 1: Простая подстановка переменной
    [TestMethod]
    public void Test_SimpleVariable_ShouldReplace()
    {
        var template = "<h1>Привет, ${Name}!</h1>";
        var data = new { Name = "Иван" };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Привет, Иван!"));
    }

    // Тест 2: Несколько переменных
    [TestMethod]
    public void Test_MultipleVariables_ShouldReplaceAll()
    {
        var template = "<p>${Name} работает в ${Company} уже ${Years} лет</p>";
        var data = new { Name = "Петр", Company = "Google", Years = 5 };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Петр работает в Google уже 5 лет"));
    }

    // Тест 3: Условие истинно
    [TestMethod]
    public void Test_IfTrue_ShouldShowTrueBranch()
    {
        var template = @"$if(IsActive)
<span>Пользователь активен</span>
$else
<span>Пользователь неактивен</span>
$endif";
        var data = new { IsActive = true };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Пользователь активен"));
        Assert.IsFalse(result.Contains("Пользователь неактивен"));
    }

    // Тест 4: Условие ложно
    [TestMethod]
    public void Test_IfFalse_ShouldShowElseBranch()
    {
        var template = @"$if(IsActive)
<span>Активен</span>
$else
<span>Неактивен</span>
$endif";
        var data = new { IsActive = false };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Неактивен"));
        Assert.IsFalse(result.Contains("Активен"));
    }

    // Тест 5: Условие без else
    [TestMethod]
    public void Test_IfWithoutElse_ShouldWork()
    {
        var template = @"Начало
$if(ShowMessage)
<p>Сообщение показано</p>
$endif
Конец";
        var data = new { ShowMessage = true };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Начало"));
        Assert.IsTrue(result.Contains("Сообщение показано"));
        Assert.IsTrue(result.Contains("Конец"));
    }

    // Тест 6: Сравнение чисел больше
    [TestMethod]
    public void Test_NumberComparison_Greater_ShouldWork()
    {
        var template = @"$if(Age >= 18)
<p>Совершеннолетний</p>
$else
<p>Несовершеннолетний</p>
$endif";
        var data = new { Age = 25 };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Совершеннолетний"));
    }

    // Тест 7: Сравнение чисел меньше
    [TestMethod]
    public void Test_NumberComparison_Less_ShouldWork()
    {
        var template = @"$if(Age < 18)
<p>Ребенок</p>
$else
<p>Взрослый</p>
$endif";
        var data = new { Age = 16 };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Ребенок"));
    }

    // Тест 8: Простой цикл
    [TestMethod]
    public void Test_SimpleForeach_ShouldRenderItems()
    {
        var template = @"$foreach(var item in Items)
<li>${item.Name}</li>
$endfor";
        var data = new { Items = new[] { new { Name = "Яблоко" }, new { Name = "Банан" }, new { Name = "Апельсин" } } };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Яблоко"));
        Assert.IsTrue(result.Contains("Банан"));
        Assert.IsTrue(result.Contains("Апельсин"));
    }

    // Тест 9: Цикл с несколькими свойствами
    [TestMethod]
    public void Test_ForeachMultipleProperties_ShouldWork()
    {
        var template = @"$foreach(var product in Products)
<div>${product.Name} - ${product.Price} руб.</div>
$endfor";
        var data = new { Products = new[] { 
            new { Name = "Хлеб", Price = 30 }, 
            new { Name = "Молоко", Price = 60 } 
        }};
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Хлеб - 30 руб."));
        Assert.IsTrue(result.Contains("Молоко - 60 руб."));
    }

    // Тест 10: Пустой цикл
    [TestMethod]
    public void Test_EmptyForeach_ShouldRenderNothing()
    {
        var template = @"Начало
$foreach(var item in Items)
<p>${item.Name}</p>
$endfor
Конец";
        var data = new { Items = new object[] { } };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Начало"));
        Assert.IsTrue(result.Contains("Конец"));
        Assert.IsFalse(result.Contains("<p>"));
    }

    // Тест 11: Несуществующая переменная
    [TestMethod]
    public void Test_UnknownVariable_ShouldReturnEmpty()
    {
        var template = "<p>Имя: ${UnknownProperty}</p>";
        var data = new { Name = "Иван" };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Имя: "));
        Assert.IsFalse(result.Contains("Иван"));
    }

    // Тест 12: Комбинированный шаблон
    [TestMethod]
    public void Test_CombinedTemplate_ShouldWork()
    {
        var template = @"<div>
    <h1>Пользователь: ${Name}</h1>
$if(IsActive)
    <p>Статус: Активен</p>
    <ul>
$foreach(var item in Items)
        <li>${item.Title} - ${item.Count} шт.</li>
$endfor
    </ul>
$else
    <p>Пользователь неактивен</p>
$endif
</div>";
        
        var data = new { 
            Name = "Анна", 
            IsActive = true, 
            Items = new[] { 
                new { Title = "Книга", Count = 3 },
                new { Title = "Ручка", Count = 5 }
            }
        };
        
        var result = _renderer.RenderFromString(template, data);
        
        // Проверяем что основные элементы присутствуют
        Assert.IsTrue(result.Contains("Пользователь: Анна"));
        Assert.IsTrue(result.Contains("Статус: Активен"));
        Assert.IsTrue(result.Contains("Книга - 3 шт."));
        Assert.IsTrue(result.Contains("Ручка - 5 шт."));
    }

    // Тест 13: Пустой шаблон
    [TestMethod]
    public void Test_EmptyTemplate_ShouldReturnEmpty()
    {
        var template = "";
        var data = new { Name = "Тест" };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(string.IsNullOrEmpty(result.Trim()));
    }

    // Тест 14: Только текст без переменных
    [TestMethod]
    public void Test_PlainText_ShouldReturnAsIs()
    {
        var template = "<h1>Обычный HTML без переменных</h1>";
        var data = new { Name = "Тест" };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Обычный HTML без переменных"));
    }

    // Тест 15: Работа с файлами
    [TestMethod]
    public void Test_RenderFromFile_ShouldWork()
    {
        var fileName = "test_template.html";
        var template = "<p>Привет, ${Name}!</p>";
        
        // Создаем временный файл
        File.WriteAllText(fileName, template);
        
        try
        {
            var data = new { Name = "Мир" };
            var result = _renderer.RenderFromFile(fileName, data);
            
            Assert.IsTrue(result.Contains("Привет, Мир!"));
        }
        finally
        {
            // Удаляем файл
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    // Тест 16: Сохранение в файл
    [TestMethod]
    public void Test_RenderToFile_ShouldCreateFile()
    {
        var inputFile = "input_test.html";
        var outputFile = "output_test.html";
        var template = "<h1>${Title}</h1><p>${Content}</p>";
        
        // Создаем входной файл
        File.WriteAllText(inputFile, template);
        
        try
        {
            var data = new { Title = "Заголовок", Content = "Содержимое страницы" };
            var result = _renderer.RenderToFile(inputFile, outputFile, data);
            
            // Проверяем что файл создался и содержит правильные данные
            Assert.IsTrue(File.Exists(outputFile));
            var fileContent = File.ReadAllText(outputFile);
            Assert.IsTrue(fileContent.Contains("Заголовок"));
            Assert.IsTrue(fileContent.Contains("Содержимое страницы"));
            Assert.AreEqual(fileContent, result);
        }
        finally
        {
            // Удаляем файлы
            if (File.Exists(inputFile))
                File.Delete(inputFile);
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }

    // Тест 17: Проверка булевых значений
    [TestMethod]
    public void Test_BooleanConditions_ShouldWork()
    {
        var template = @"$if(IsAdmin)
<p>Администратор</p>
$endif
$if(!IsGuest)
<p>Не гость</p>
$endif";
        var data = new { IsAdmin = true, IsGuest = false };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Администратор"));
        Assert.IsTrue(result.Contains("Не гость"));
    }

    // Тест 18: Проверка строковых сравнений
    [TestMethod]
    public void Test_StringComparison_ShouldWork()
    {
        var template = @"$if(Role == admin)
<p>Роль: Администратор</p>
$else
<p>Роль: Пользователь</p>
$endif";
        var data = new { Role = "admin" };
        
        var result = _renderer.RenderFromString(template, data);
        
        Assert.IsTrue(result.Contains("Роль: Администратор"));
    }
}