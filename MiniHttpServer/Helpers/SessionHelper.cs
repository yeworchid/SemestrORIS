using System.Net;
using MiniHttpServer.Models;
using MiniHttpServer.Services;

namespace MiniHttpServer.Helpers
{
    // хелпер для работы с сессиями
    public static class SessionHelper
    {
        // получаем текущего юзера из cookie
        public static User? GetCurrentUser(HttpListenerContext context)
        {
            var cookie = context.Request.Cookies["session_token"];
            if (cookie == null)
            {
                return null;
            }
            
            return SessionManager.GetUser(cookie.Value);
        }

        // устанавливаем cookie с токеном сессии
        public static void SetSessionCookie(HttpListenerContext context, string token)
        {
            var cookie = new Cookie("session_token", token)
            {
                HttpOnly = true,
                Path = "/"
            };
            context.Response.Cookies.Add(cookie);
        }

        // удаляем cookie сессии
        public static void RemoveSessionCookie(HttpListenerContext context)
        {
            var cookie = new Cookie("session_token", "")
            {
                Expires = DateTime.Now.AddDays(-1),
                Path = "/"
            };
            context.Response.Cookies.Add(cookie);
        }
    }
}
