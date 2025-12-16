# 🚀 Railway Deployment Guide

Пошаговая инструкция по деплою Bryx CRM + Bot на Railway.

## 📋 Предварительные требования

- [ ] Аккаунт на [GitHub](https://github.com)
- [ ] Аккаунт на [Railway.app](https://railway.app)
- [ ] Telegram Bot Token (от [@BotFather](https://t.me/BotFather))
- [ ] Ваш Telegram Chat ID (от [@userinfobot](https://t.me/userinfobot))
- [ ] Репозиторий проекта на GitHub

## 🎯 Архитектура деплоя

```
Railway Project
├── PostgreSQL Database (Railway Plugin)
├── Bryx CRM Service (Web App)
│   ├── Port: 8080 (автоматически)
│   ├── Volume: /app/backups (для бэкапов)
│   └── Public URL: https://your-crm.railway.app
└── Bryx Bot Service (Console App)
    └── Подключается к CRM API
```

## 📝 Шаг 1: Подготовка репозитория

### 1.1 Инициализация Git (если еще не сделано)

```bash
cd bryx_base
git init
git add .
git commit -m "Initial commit: Bryx CRM + Bot ready for Railway deployment"
```

### 1.2 Создание репозитория на GitHub

1. Перейдите на [GitHub](https://github.com/new)
2. Создайте новый репозиторий (например, `bryx-crm`)
3. **НЕ** инициализируйте с README, .gitignore или лицензией
4. Нажмите **Create repository**

### 1.3 Отправка кода на GitHub

```bash
git remote add origin https://github.com/YOUR_USERNAME/bryx-crm.git
git branch -M main
git push -u origin main
```

## 🚂 Шаг 2: Создание проекта в Railway

### 2.1 Создание проекта

1. Перейдите в [Railway Dashboard](https://railway.app/dashboard)
2. Нажмите **"+ New Project"**
3. Выберите **"Deploy from GitHub repo"**
4. Авторизуйте Railway для доступа к вашему GitHub
5. Выберите репозиторий `bryx-crm`
6. Railway создаст проект и попытается задеплоить - **временно игнорируйте это**

## 🗄️ Шаг 3: Настройка PostgreSQL

### 3.1 Добавление базы данных

1. В вашем проекте Railway нажмите **"+ New Service"**
2. Выберите **"Database"** → **"Add PostgreSQL"**
3. Railway автоматически создаст БД и сгенерирует переменную `DATABASE_URL`

### 3.2 Проверка подключения

1. Нажмите на PostgreSQL сервис
2. Перейдите во вкладку **"Variables"**
3. Найдите `DATABASE_URL` - это строка подключения к БД
4. Скопируйте её (понадобится для локального тестирования)

## 🌐 Шаг 4: Деплой CRM Сервиса

### 4.1 Настройка Build

1. Нажмите на сервис, который создал Railway при первом деплое
2. Переименуйте его в **"bryx-crm"** (Settings → Service Name)
3. Перейдите в **"Settings"** → **"Service Settings"**
4. Установите:
   - **Root Directory**: `bryx CRM/bryx CRM`
   - **Builder**: Nixpacks (по умолчанию)

### 4.2 Настройка переменных окружения

1. Перейдите во вкладку **"Variables"**
2. Нажмите **"+ New Variable"**
3. Добавьте следующие переменные:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:$PORT
TELEGRAM_BOT_TOKEN=ваш_токен_от_botfather
TELEGRAM_CHAT_ID=ваш_chat_id
```

### 4.3 Подключение PostgreSQL

1. Во вкладке **"Variables"** найдите секцию **"Service Variables"**
2. Нажмите **"+ Reference"**
3. Выберите PostgreSQL сервис
4. Выберите переменную `DATABASE_URL`
5. Railway автоматически добавит reference

**Важно**: CRM автоматически обнаружит `DATABASE_URL` и использует её для подключения к БД.

### 4.4 Создание Volume для бэкапов (опционально, но рекомендуется)

1. В настройках CRM сервиса перейдите в **"Settings"** → **"Volumes"**
2. Нажмите **"+ Add Volume"**
3. Установите:
   - **Mount Path**: `/app/backups`
   - **Size**: 1GB (или больше по необходимости)
4. Нажмите **"Add"**

### 4.5 Настройка Health Check

1. Перейдите в **"Settings"** → **"Health Check"**
2. Установите:
   - **Path**: `/`
   - **Timeout**: 300 секунд
   - **Interval**: 30 секунд
3. Нажмите **"Save"**

### 4.6 Деплой

1. Перейдите во вкладку **"Deployments"**
2. Нажмите **"Deploy"** (или Railway задеплоит автоматически при изменениях)
3. Дождитесь завершения деплоя (обычно 2-5 минут)
4. Проверьте логи во вкладке **"Logs"**

**Ожидаемые логи при успешном запуске:**
```
🚀 Starting application on port: 8080
✅ Миграции базы данных применены успешно
✅ Всего применено миграций: XX
Now listening on: http://0.0.0.0:8080
```

### 4.7 Получение публичного URL

1. Перейдите в **"Settings"** → **"Networking"**
2. Нажмите **"Generate Domain"**
3. Railway создаст публичный домен (например, `bryx-crm-production.railway.app`)
4. **Скопируйте этот URL** - он понадобится для настройки бота

## 🤖 Шаг 5: Деплой Telegram Bot

### 5.1 Создание нового сервиса

1. В проекте Railway нажмите **"+ New Service"**
2. Выберите **"GitHub Repo"**
3. Выберите тот же репозиторий `bryx-crm`
4. Переименуйте сервис в **"bryx-bot"**

### 5.2 Настройка Build

1. Перейдите в **"Settings"** → **"Service Settings"**
2. Установите:
   - **Root Directory**: `bryxBot`
   - **Builder**: Nixpacks

### 5.3 Настройка переменных окружения

1. Перейдите во вкладку **"Variables"**
2. Добавьте переменные:

```env
ASPNETCORE_ENVIRONMENT=Production
BOT_TOKEN=ваш_токен_от_botfather
CRM_API_URL=https://ваш-crm-домен.railway.app/api/bot/
ALLOWED_USERS=123456789,987654321
```

**Важно**:
- `CRM_API_URL` должен содержать публичный URL вашего CRM сервиса (из шага 4.7)
- `ALLOWED_USERS` - список Telegram ID пользователей через запятую
- Убедитесь, что URL заканчивается на `/api/bot/`

### 5.4 Деплой

1. Нажмите **"Deploy"**
2. Дождитесь завершения деплоя
3. Проверьте логи

**Ожидаемые логи при успешном запуске:**
```
Запуск Bryx Bot...
Бот запущен: @вашБот
```

## ✅ Шаг 6: Проверка работы

### 6.1 Проверка CRM

1. Откройте публичный URL вашего CRM в браузере
2. Вы должны увидеть главную страницу Bryx CRM
3. Попробуйте перейти на страницу `/warehouse`

### 6.2 Проверка бота

1. Откройте Telegram
2. Найдите вашего бота по username
3. Отправьте `/start`
4. Бот должен ответить приветственным сообщением

### 6.3 Проверка интеграции

1. Отправьте боту команду для просмотра товаров
2. Бот должен запросить данные из CRM API
3. Проверьте логи обоих сервисов в Railway

## 🔧 Шаг 7: Настройка автоматического деплоя

### 7.1 Railway автоматически деплоит при push

1. Railway уже настроен на автодеплой из ветки `main`
2. При каждом `git push` в `main`, Railway пересоберет сервисы

### 7.2 Настройка Deploy Triggers (опционально)

1. Перейдите в **"Settings"** → **"Deploy Triggers"**
2. Можно настроить деплой только при изменениях в определенных директориях:
   - CRM сервис: деплой только при изменениях в `bryx CRM/`
   - Bot сервис: деплой только при изменениях в `bryxBot/`

## 🐛 Troubleshooting

### Проблема: CRM не подключается к БД

**Решение**:
1. Проверьте, что переменная `DATABASE_URL` присутствует в Variables
2. Проверьте логи на наличие ошибок подключения
3. Убедитесь, что PostgreSQL сервис запущен

### Проблема: Бот не может подключиться к CRM API

**Решение**:
1. Проверьте `CRM_API_URL` - должен быть публичный URL, а не localhost
2. Убедитесь, что URL заканчивается на `/api/bot/`
3. Проверьте, что CRM сервис запущен и доступен
4. Проверьте логи CRM на наличие входящих запросов от бота

### Проблема: Миграции не применяются

**Решение**:
1. CRM автоматически применяет миграции при запуске
2. Проверьте логи на наличие ошибок миграций
3. Можно применить миграции вручную через Railway CLI:
   ```bash
   railway run dotnet ef database update --project "bryx CRM/bryx CRM"
   ```

### Проблема: Бот отвечает "Unauthorized"

**Решение**:
1. Проверьте, что ваш Telegram ID добавлен в `ALLOWED_USERS`
2. Получите свой ID через [@userinfobot](https://t.me/userinfobot)
3. Обновите переменную `ALLOWED_USERS` в настройках бота
4. Перезапустите бот сервис

### Проблема: 502 Bad Gateway при доступе к CRM

**Решение**:
1. Проверьте, что сервис запущен (зеленый индикатор)
2. Проверьте логи на наличие ошибок
3. Увеличьте Health Check Timeout в настройках
4. Убедитесь, что приложение слушает на `$PORT` (Railway автоматически назначает порт)

## 📊 Мониторинг

### Логи

1. **Real-time логи**: Перейдите в сервис → **"Logs"**
2. **Исторические логи**: Доступны во вкладке **"Deployments"** → выбрать деплой → **"Logs"**

### Метрики

1. Перейдите в сервис → **"Metrics"**
2. Доступны:
   - CPU Usage
   - Memory Usage
   - Network I/O
   - Request Count

### Алерты

1. Railway может отправлять уведомления в Discord или email
2. Настройте в **Project Settings** → **"Integrations"**

## 💰 Стоимость

Railway предоставляет:
- **$5 бесплатных кредитов** ежемесячно
- **500 часов выполнения** на Hobby план
- Для продакшена рекомендуется **Pro план** ($20/месяц)

**Примерное потребление для этого проекта**:
- PostgreSQL: ~$1-2/месяц
- CRM сервис: ~$2-3/месяц
- Bot сервис: ~$0.50-1/месяц
- **Итого**: ~$3.50-6/месяц (вписывается в бесплатные $5)

## 🔐 Безопасность

### Рекомендации:

1. ✅ **Никогда** не коммитьте `appsettings.json` с реальными токенами
2. ✅ Используйте Environment Variables для всех секретов
3. ✅ Регулярно обновляйте Telegram Bot Token
4. ✅ Ограничьте список `ALLOWED_USERS` только доверенными пользователями
5. ✅ Включите 2FA на GitHub и Railway
6. ✅ Используйте сильные пароли для PostgreSQL (Railway генерирует автоматически)

## 🎉 Готово!

Поздравляем! Ваш Bryx CRM + Bot успешно задеплоен на Railway.

### Полезные ссылки:

- 📖 [Railway Documentation](https://docs.railway.app)
- 💬 [Railway Discord](https://discord.gg/railway)
- 🤖 [Telegram Bot API](https://core.telegram.org/bots/api)
- 🔧 [ASP.NET Core Deployment](https://docs.microsoft.com/aspnet/core/host-and-deploy)

---

**Нужна помощь?** Создайте Issue в репозитории или свяжитесь с разработчиком.
