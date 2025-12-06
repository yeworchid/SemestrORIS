using MiniHttpServer.Framework.Settings;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using MiniHttpServer.Framework.Server;
using MiniTemplateEngine;
using MiniHttpServer.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        JsonEntity settings = null;

        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        await Task.Run(async () =>
        {
            try
            {
                var settings = Singleton.GetInstance().Settings;

                if (settings == null)
                {
                    Console.WriteLine("Ошибка: настройки не загружены");
                    return;
                }

                if (!System.IO.File.Exists("Public/index.html"))
                    Console.WriteLine($"Файл Public/index.html не найден");

                HttpServer server = new HttpServer(settings);

                Console.WriteLine($"Запуск сервера на {settings.Domain}:{settings.Port}");
                server.Start(token);

                Console.WriteLine("Сервер запущен");
                while (!token.IsCancellationRequested)
                {
                    var input = Console.ReadLine();
                    if (input == "/stop")
                    {
                        Console.WriteLine("Получена команда остановки...");
                        cts.Cancel();
                        break;
                    }
                }

                server.Stop();
                Console.WriteLine("Сервер остановлен");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Файл settings.json не найден");
                return;
            }
            catch (JsonException)
            {
                Console.WriteLine("Ошибка формата JSON");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
                return;
            }
        });
    }
}