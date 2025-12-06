# сборка приложения
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# копируем все проекты
COPY ["MiniHttpServer/MiniHttpServer.csproj", "MiniHttpServer/"]
COPY ["MiniHttpServer.Framework/MiniHttpServer.Framework.csproj", "MiniHttpServer.Framework/"]
COPY ["MiniTemplateEngine/MiniTemplateEngine.csproj", "MiniTemplateEngine/"]
COPY ["MyORMLibrary/MyORMLibrary.csproj", "MyORMLibrary/"]

# восстанавливаем зависимости
RUN dotnet restore "MiniHttpServer/MiniHttpServer.csproj"

# копируем весь код
COPY . .

# собираем проект
WORKDIR "/src/MiniHttpServer"
RUN dotnet publish "MiniHttpServer.csproj" -c Release -o /app/publish

# финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# копируем settings.json из исходников если он не был скопирован при publish
COPY --from=build /src/MiniHttpServer/settings.json ./settings.json

# открываем порт для http сервера
EXPOSE 1337

ENTRYPOINT ["dotnet", "MiniHttpServer.dll"]