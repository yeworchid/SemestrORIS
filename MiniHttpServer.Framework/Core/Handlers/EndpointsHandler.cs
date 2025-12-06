using MiniHttpServer.Framework.Core.Abstracts;
using MiniHttpServer.Framework.Core.Attributes;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace MiniHttpServer.Framework.Core.Handlers
{
    internal class EndpointsHandler : Handler
    {
        public override async void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var pathSegments = request.Url?.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (pathSegments == null || pathSegments.Length == 0)
                {
                    if (Successor != null)
                        Successor.HandleRequest(context);
                    return;
                }

                var endpointName = pathSegments[0];

                var assembly = Assembly.GetEntryAssembly();
                var endpont = assembly?.GetTypes()
                                       .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                                       .FirstOrDefault(end => IsCheckedNameEndpoint(end.Name, endpointName));

                if (endpont == null) 
                {
                    if (Successor != null)
                        Successor.HandleRequest(context);
                    return;
                }

                var method = endpont.GetMethods().Where(t => t.GetCustomAttributes(true)
                            .Any(attr => attr.GetType().Name.Equals($"Http{context.Request.HttpMethod}", 
                                                                    StringComparison.OrdinalIgnoreCase)))
                            .FirstOrDefault();

                if (method == null) 
                {
                    if (Successor != null)
                        Successor.HandleRequest(context);
                    return;
                }

                // создаем экземпляр endpoint класса
                var endpointInstance = Activator.CreateInstance(endpont);
                
                // если endpoint наследуется от BaseEndpoint, передаем ему контекст
                if (endpointInstance is MiniHttpServer.Framework.Core.HttpResponse.BaseEndpoint baseEndpoint)
                {
                    baseEndpoint.SetContext(context);
                }
                
                // вызываем метод endpoint'а
                object ret;
                var parameters = method.GetParameters();
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(HttpListenerContext))
                {
                    // если метод принимает HttpListenerContext, передаем его
                    ret = method.Invoke(endpointInstance, new object[] { context });
                }
                else
                {
                    // иначе вызываем без параметров
                    ret = method.Invoke(endpointInstance, null);
                }
                
                // Если метод асинхронный, ждем результат
                if (ret is Task task)
                {
                    await task;
                    if (task.GetType().IsGenericType)
                    {
                        ret = ((dynamic)task).Result;
                    }
                    else
                    {
                        ret = null;
                    }
                }
                
                if (ret is string stringResult)
                {
                    if (stringResult.EndsWith(".html") || stringResult.EndsWith(".css") || stringResult.EndsWith(".js"))
                    {
                        // загружаем файл
                        var buffer = MiniHttpServer.Framework.Shared.GetResponseBytes.Invoke(stringResult);
                        if (buffer != null)
                        {
                            context.Response.ContentType = MiniHttpServer.Framework.Shared.ContentType.GetContentType(stringResult);
                            context.Response.ContentLength64 = buffer.Length;
                            using var output = context.Response.OutputStream;
                            await output.WriteAsync(buffer, 0, buffer.Length);
                            await output.FlushAsync();
                        }
                    }
                    else
                    {
                        // иначе это просто текст - отправляем как есть
                        await SendResponse(context.Response, stringResult);
                    }
                }
                else if (ret is MiniHttpServer.Framework.Core.HttpResponse.IResponseResult responseResult)
                {
                    responseResult.Execute(context);
                }
                else
                {
                    // если метод ничего не вернул
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в endpoint: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // отправляем страницу с ошибкой
                MiniHttpServer.Framework.Core.ErrorPageHelper.SendErrorPage(context);
            }
        }

        private bool IsCheckedNameEndpoint(string endpointName, string className) =>
            endpointName.Equals(className, StringComparison.OrdinalIgnoreCase) ||
            endpointName.Equals($"{className}Endpoint", StringComparison.OrdinalIgnoreCase);

        private async Task SendResponse(HttpListenerResponse response, string content)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
            // получаем поток ответа и пишем в него ответ
            response.ContentLength64 = buffer.Length;
            using var output = response.OutputStream;
            // отправляем данные
            await output.WriteAsync(buffer);
            await output.FlushAsync();
        }
    }
}
