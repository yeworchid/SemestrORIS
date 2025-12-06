using MiniHttpServer.Framework.Settings;
using MiniHttpServer.Framework.Core.HttpResponse;
using MiniHttpServer.Framework.Core.Attributes;
using MiniHttpServer.Repositories;
using MyORMLibrary;

[Endpoint]
internal class UserEndpoint : BaseEndpoint
{
    private readonly UserRepository _userRepository;

    public UserEndpoint()
    {
        var settings = Singleton.GetInstance().Settings;
        var orm = new ORMContext(settings.ConnectionString);
        _userRepository = new UserRepository(orm);
    }

    [HttpGet]
    public async Task<IResponseResult> GetUsers()
    {
        var users = await _userRepository.GetAllAsync();
    
        // возвращаем HTML страницу с данными пользователей
        return Page("Templates/Pages/users.thtml", new { users });
    }
}