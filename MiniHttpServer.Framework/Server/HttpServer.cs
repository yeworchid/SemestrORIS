using MiniHttpServer.Framework.Core.Abstracts;
using MiniHttpServer.Framework.Core.Handlers;
using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Shared;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace MiniHttpServer.Framework.Server 
{
    public class HttpServer
    {
        private HttpListener _listener = new();
        private JsonEntity _config;
        private CancellationToken _token;

        public HttpServer(JsonEntity config) { _config = config; }

        public void Start(CancellationToken token)
        {
            _token = token;
            _listener = new HttpListener();
            // В Docker используем + для прослушивания на всех интерфейсах
            string domain = _config.Domain == "localhost" || _config.Domain == "127.0.0.1" ? "+" : _config.Domain;
            string url = "http://" + domain + ":" + _config.Port + "/";
            _listener.Prefixes.Add(url);
            _listener.Start();
            Console.WriteLine("Сервер запущен! Проверяй в браузере: http://localhost:" + _config.Port + "/");
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        protected async void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening && !_token.IsCancellationRequested)
            {
                HttpListenerContext context = null;
                try
                {
                    context = _listener.EndGetContext(result);
                    Console.WriteLine($"Получен запрос: {context.Request.HttpMethod} {context.Request.Url?.AbsolutePath}");

                    Handler staticFilesHandler = new StaticFilesHandler();
                    Handler endpointsHandler = new EndpointsHandler();
                    Handler notFoundHandler = new NotFoundHandler();
                    
                    staticFilesHandler.Successor = endpointsHandler;
                    endpointsHandler.Successor = notFoundHandler;
                    
                    staticFilesHandler.HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Ошибка обработки запроса: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // отправляем страницу с ошибкой вместо падения сервера
                    if (context != null && context.Response != null)
                    {
                        MiniHttpServer.Framework.Core.ErrorPageHelper.SendErrorPage(context);
                    }
                }
                finally
                {
                    // всегда продолжаем слушать новые запросы
                    if (!_token.IsCancellationRequested)
                        Receive();
                }
            }
        }


    }
}