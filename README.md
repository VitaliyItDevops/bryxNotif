# 🚀 Bryx CRM + Telegram Bot

Комплексная система управления складом с веб-интерфейсом и Telegram ботом для удобного управления товарами.

## 📋 Описание проекта

Проект состоит из двух компонентов:

1. **Bryx CRM** - веб-приложение на ASP.NET Core Blazor Server для управления складом
2. **Bryx Bot** - Telegram бот для взаимодействия с CRM через мессенджер

### Основные возможности

#### CRM Система:
- ✅ Управление товарами и категориями
- ✅ Отслеживание продаж и закупок
- ✅ Учет движения товаров
- ✅ Система избранного и маркировки дефектных товаров
- ✅ Автоматические бэкапы базы данных
- ✅ Уведомления в Telegram
- ✅ PostgreSQL база данных

#### Telegram Bot:
- ✅ Просмотр списка товаров
- ✅ Добавление новых товаров
- ✅ Поиск по товарам
- ✅ Управление категориями
- ✅ Авторизация пользователей
- ✅ Интеграция с CRM API

## 🛠️ Технологический стек

- **Backend**: .NET 9.0, ASP.NET Core Blazor Server
- **Database**: PostgreSQL with Entity Framework Core
- **Bot Framework**: Telegram.Bot SDK
- **Hosting**: Railway (рекомендуется)
- **CI/CD**: GitHub Actions (опционально)

## 📦 Структура проекта

```
bryx_base/
├── bryx CRM/              # CRM веб-приложение
│   ├── bryx CRM/
│   │   ├── Components/    # Blazor компоненты
│   │   ├── Data/          # Модели и DbContext
│   │   ├── Services/      # Бизнес-логика и сервисы
│   │   ├── appsettings.Production.json
│   │   └── Program.cs
│   └── CLAUDE.md          # Документация для разработки
├── bryxBot/               # Telegram бот
│   ├── BryxBot/
│   │   ├── Services/      # Сервисы бота
│   │   ├── Handlers/      # Обработчики команд
│   │   ├── appsettings.Production.json
│   │   └── Program.cs
│   └── README.md
├── .gitignore
├── .env.example
├── railway.toml
├── docker-compose.yml
└── README.md              # Этот файл
```

## 🚀 Быстрый старт

### Локальная разработка

#### Требования:
- .NET 9.0 SDK
- PostgreSQL 14+
- Telegram Bot Token (от [@BotFather](https://t.me/BotFather))

#### 1. Клонирование репозитория

```bash
git clone https://github.com/your-username/bryx-crm.git
cd bryx-crm
```

#### 2. Настройка окружения

Скопируйте `.env.example` в `.env` и заполните значения:

```bash
cp .env.example .env
```

Отредактируйте `.env`:
```env
DATABASE_URL=postgresql://postgres:password@localhost:5432/bryx_crm
TELEGRAM_BOT_TOKEN=your_bot_token_here
TELEGRAM_CHAT_ID=your_chat_id
ALLOWED_USERS=123456789,987654321
CRM_API_URL=http://localhost:5092/api/bot/
```

#### 3. Запуск через Docker Compose (рекомендуется)

```bash
docker-compose up -d
```

#### 4. Запуск вручную

**Запуск PostgreSQL:**
```bash
docker run -d \
  --name bryx-postgres \
  -e POSTGRES_PASSWORD=your_password \
  -e POSTGRES_DB=bryx_crm \
  -p 5432:5432 \
  postgres:14
```

**Запуск CRM:**
```bash
cd "bryx CRM/bryx CRM"
dotnet restore
dotnet ef database update
dotnet run
```

**Запуск Bot (в отдельном терминале):**
```bash
cd bryxBot/BryxBot
dotnet restore
dotnet run
```

CRM будет доступен на: http://localhost:5092

## 🌐 Деплой на Railway

### Подготовка

1. Создайте аккаунт на [Railway.app](https://railway.app)
2. Установите Railway CLI (опционально):
   ```bash
   npm i -g @railway/cli
   ```

### Шаги деплоя

#### 1. Создание проекта в Railway

1. Зайдите в [Railway Dashboard](https://railway.app/dashboard)
2. Нажмите **"New Project"**
3. Выберите **"Deploy from GitHub repo"**
4. Авторизуйте Railway для доступа к вашему GitHub
5. Выберите репозиторий `bryx-crm`

#### 2. Настройка PostgreSQL

1. В вашем проекте нажмите **"New Service"** → **"Database"** → **"PostgreSQL"**
2. Railway автоматически создаст базу данных и переменную `DATABASE_URL`

#### 3. Деплой CRM Сервиса

1. Нажмите **"New Service"** → **"GitHub Repo"**
2. Выберите ваш репозиторий
3. В настройках сервиса:
   - **Root Directory**: `bryx CRM/bryx CRM`
   - **Build Command**: `dotnet publish -c Release -o /app/publish`
   - **Start Command**: `dotnet /app/publish/bryx_CRM.dll`

4. Добавьте переменные окружения:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   TELEGRAM_BOT_TOKEN=your_bot_token
   TELEGRAM_CHAT_ID=your_chat_id
   ```

5. Подключите PostgreSQL:
   - Railway автоматически подключит `DATABASE_URL`

6. Создайте Volume для бэкапов (опционально):
   - Перейдите в **"Settings"** → **"Volumes"**
   - Нажмите **"Add Volume"**
   - Mount Path: `/app/backups`

#### 4. Деплой Telegram Bot

1. Нажмите **"New Service"** → **"GitHub Repo"**
2. Выберите тот же репозиторий
3. В настройках сервиса:
   - **Root Directory**: `bryxBot`
   - **Build Command**: `dotnet publish BryxBot/BryxBot.csproj -c Release -o /app/publish`
   - **Start Command**: `dotnet /app/publish/BryxBot.dll`

4. Добавьте переменные окружения:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   BOT_TOKEN=your_bot_token
   CRM_API_URL=https://your-crm-service.railway.app/api/bot/
   ALLOWED_USERS=123456789,987654321
   ```

   > **Важно**: `CRM_API_URL` должен содержать публичный URL вашего CRM сервиса в Railway

#### 5. Применение миграций

После первого деплоя CRM, миграции применятся автоматически при запуске приложения.

Если нужно применить вручную:
```bash
railway run dotnet ef database update --project "bryx CRM/bryx CRM"
```

### Получение URL сервисов

1. Откройте CRM сервис в Railway
2. Перейдите в **"Settings"** → **"Networking"**
3. Скопируйте **Public Domain** (например, `https://bryx-crm-production.railway.app`)
4. Используйте этот URL для `CRM_API_URL` в настройках бота

## 🔒 Безопасность

### Переменные окружения

**НИКОГДА** не коммитьте файлы с секретными данными:
- ❌ `appsettings.json` с реальными токенами
- ❌ `.env` файлы
- ❌ Пароли базы данных

Используйте:
- ✅ `.env.example` как шаблон
- ✅ `appsettings.Production.json` без секретов
- ✅ Railway Environment Variables для production

### Получение Telegram токенов

1. **Bot Token**:
   - Напишите [@BotFather](https://t.me/BotFather)
   - Отправьте `/newbot`
   - Следуйте инструкциям
   - Скопируйте полученный токен

2. **Chat ID**:
   - Напишите [@userinfobot](https://t.me/userinfobot)
   - Отправьте `/start`
   - Бот отправит ваш Telegram ID

3. **Allowed Users**:
   - Получите Telegram ID всех пользователей через [@userinfobot](https://t.me/userinfobot)
   - Добавьте их через запятую в `ALLOWED_USERS`

## 🛠️ Разработка

### Запуск тестов

```bash
# CRM tests
cd "bryx CRM"
dotnet test

# Bot tests
cd bryxBot
dotnet test
```

### Создание миграций

```bash
cd "bryx CRM/bryx CRM"
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Форматирование кода

```bash
dotnet format
```

## 📚 Документация

- [Blazor Server Docs](https://docs.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Telegram Bot API](https://core.telegram.org/bots/api)
- [Railway Docs](https://docs.railway.app)

### Документация проекта

- `bryx CRM/CLAUDE.md` - Детальная документация CRM системы
- `bryxBot/README.md` - Документация Telegram бота

## 🐛 Отладка

### Логи в Railway

1. Откройте сервис в Railway Dashboard
2. Перейдите в **"Deployments"**
3. Нажмите на последний деплой
4. Просмотрите **"Logs"**

### Локальные логи

```bash
# CRM
cd "bryx CRM/bryx CRM"
dotnet run --verbosity detailed

# Bot
cd bryxBot/BryxBot
dotnet run --verbosity detailed
```

## 🤝 Вклад в проект

1. Fork репозитория
2. Создайте feature ветку (`git checkout -b feature/AmazingFeature`)
3. Commit изменений (`git commit -m 'Add some AmazingFeature'`)
4. Push в ветку (`git push origin feature/AmazingFeature`)
5. Откройте Pull Request

## 📝 Лицензия

Распространяется под лицензией MIT. См. `LICENSE` для подробностей.

## 👤 Автор

BRYX - [@your_telegram](https://t.me/your_username)

## 🙏 Благодарности

- [Telegram Bot SDK](https://github.com/TelegramBots/Telegram.Bot)
- [Railway](https://railway.app) за отличный хостинг
- [ASP.NET Core Team](https://github.com/dotnet/aspnetcore)

---

**Сделано с ❤️ и .NET**
