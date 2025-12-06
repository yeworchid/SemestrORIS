using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Repositories;
using MyORMLibrary;

[Endpoint]
internal class OrderEndpoint : BaseEndpoint
{
    [HttpPost]
    public async Task<string> Order()
    {
        // читаем данные из POST body
        string requestBody;
        using (var reader = new StreamReader(Context.Request.InputStream))
        {
            requestBody = await reader.ReadToEndAsync();
        }
        
        Console.WriteLine("Получены данные заказа: " + requestBody);
        
        // парсим form data (формат: tourId=1&date=2&city=3)
        var formData = new Dictionary<string, string>();
        var parts = requestBody.Split('&');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2)
            {
                var key = System.Web.HttpUtility.UrlDecode(keyValue[0]);
                var value = System.Web.HttpUtility.UrlDecode(keyValue[1]);
                formData[key] = value;
            }
        }
        
        var tourId = formData.ContainsKey("tourId") ? formData["tourId"] : "";
        var date = formData.ContainsKey("date") ? formData["date"] : "";
        var city = formData.ContainsKey("city") ? formData["city"] : "";
        
        Console.WriteLine($"Заказ: tourId={tourId}, date={date}, city={city}");
        
        // перенаправляем на страницу тура с параметрами
        Context.Response.Redirect($"/travel?id={tourId}&date={date}&city={city}");
        return "";
    }
}