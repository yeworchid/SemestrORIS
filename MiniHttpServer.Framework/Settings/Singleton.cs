using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Text.Json;

namespace MiniHttpServer.Framework.Settings
{
    public class Singleton
    {
        private static Singleton instance;
        public JsonEntity Settings { get; private set; }
        private Singleton()
        {
            var json = File.ReadAllText("settings.json");
            Settings = JsonSerializer.Deserialize<JsonEntity>(json);
            
            // переопределяем настройки из переменных окружения (для Docker)
            var envConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
            if (!string.IsNullOrEmpty(envConnectionString))
            {
                Settings.ConnectionString = envConnectionString;
                Console.WriteLine($"ConnectionString переопределен из переменной окружения: {envConnectionString}");
            }
        }

        public static Singleton GetInstance()
        {
            if (instance == null)
            {
                instance = new Singleton();
            }
            return instance;
        }

    }
}
