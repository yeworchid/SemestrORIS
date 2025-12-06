using MiniHttpServer.Models;

namespace MiniHttpServer.Services
{
    // простой менеджер сессий в памяти
    public static class SessionManager
    {
        // словарь токен -> юзер
        private static Dictionary<string, User> _sessions = new Dictionary<string, User>();

        // добавляем сессию
        public static string CreateSession(User user)
        {
            string token = Guid.NewGuid().ToString();
            _sessions[token] = user;
            Console.WriteLine($"создана сессия для {user.Email}, токен: {token}");
            return token;
        }

        // получаем юзера по токену
        public static User? GetUser(string token)
        {
            if (_sessions.ContainsKey(token))
            {
                return _sessions[token];
            }
            return null;
        }

        // удаляем сессию (для логаута)
        public static void RemoveSession(string token)
        {
            if (_sessions.ContainsKey(token))
            {
                _sessions.Remove(token);
                Console.WriteLine($"удалена сессия: {token}");
            }
        }
    }
}
