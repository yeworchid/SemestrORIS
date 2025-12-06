# MiniHttpServer - Туристическое агентство

## Быстрый старт

### Требования
- Docker
- Docker Compose

### Запуск

В корне проекта нужно выполнить команду

```bash
docker-compose up --build
```

Приложение будет доступно по адресу: http://localhost:1337

### Что происходит при запуске

1. Запускается PostgreSQL контейнер
2. Автоматически создаются все таблицы (из `init.sql`)
3. Автоматически заполняются тестовые данные (из `seed.sql`)
4. Запускается веб-сервер

### Остановка

```bash
docker-compose down
```

Для полной очистки (включая данные БД):
```bash
docker-compose down -v
```

## Структура проекта

- `MiniHttpServer/` - основное приложение
- `MiniHttpServer.Framework/` - фреймворк для HTTP сервера
- `MiniTemplateEngine/` - шаблонизатор
- `MyORMLibrary/` - ORM
- `MiniHttpServer/Database/` - SQL скрипты для инициализации БД

## Порты

- `1337` - веб-сервер
- `5433` - PostgreSQL (внешний порт)

# Проверка функционала

## Авторизация

Производится по адресу http://localhost:1337/auth

### Тестовые пользователи

**Администратор:**
- Email: kaleevbd@gmail.com
- Пароль: password123

**Обычный пользователь:**
- Email: user@example.de
- Пароль: password123

## Админ панель

Находится по адресу http://localhost:1337/admin