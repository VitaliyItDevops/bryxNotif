# ✅ Чеклист перед деплоем на Railway

Используйте этот чеклист, чтобы убедиться, что проект готов к загрузке на GitHub и деплою на Railway.

## 🔐 Безопасность (КРИТИЧНО!)

### ⚠️ Проверьте файлы с секретными данными

- [ ] **ВАЖНО**: Убедитесь, что `.gitignore` правильно настроен
- [ ] Проверьте, что `appsettings.json` **НЕ** будет закоммичен (должен быть в .gitignore)
- [ ] Убедитесь, что файлы `.env` **НЕ** будут закоммичены

### 🔍 Команда для проверки

Выполните следующую команду, чтобы увидеть, какие файлы будут закоммичены:

```bash
git status
git add -A --dry-run
```

**Убедитесь, что в списке НЕТ:**
- ❌ `appsettings.json` (с реальными токенами)
- ❌ `.env` файлов
- ❌ `Backups/` директории
- ❌ Файлов с паролями или токенами

**Должны быть ТОЛЬКО:**
- ✅ `appsettings.Production.json` (без секретов)
- ✅ `appsettings.Development.json.template` (шаблон)
- ✅ `.env.example` (шаблон без реальных значений)

## 📋 Предварительная подготовка

### 1. Telegram Bot

- [ ] Создан бот через [@BotFather](https://t.me/BotFather)
- [ ] Получен и сохранен Bot Token
- [ ] Получен ваш Chat ID через [@userinfobot](https://t.me/userinfobot)
- [ ] Получены Chat ID всех пользователей, которым нужен доступ

### 2. GitHub

- [ ] Создан аккаунт на GitHub
- [ ] Создан новый репозиторий (например, `bryx-crm`)
- [ ] Репозиторий **пустой** (без README, .gitignore, лицензии)

### 3. Railway

- [ ] Создан аккаунт на [Railway.app](https://railway.app)
- [ ] Railway авторизован для доступа к GitHub

## 🛠️ Проверка файлов проекта

### ✅ Обязательные файлы в корне

- [ ] `.gitignore` - существует и правильно настроен
- [ ] `.env.example` - существует (шаблон переменных окружения)
- [ ] `README.md` - существует (основная документация)
- [ ] `DEPLOYMENT.md` - существует (инструкция по деплою)
- [ ] `docker-compose.yml` - существует (для локальной разработки)
- [ ] `railway.toml` - существует (конфигурация Railway)

### ✅ Файлы CRM проекта

- [ ] `bryx CRM/Dockerfile` - существует
- [ ] `bryx CRM/bryx CRM/appsettings.Production.json` - существует (БЕЗ секретов)
- [ ] `bryx CRM/bryx CRM/appsettings.Development.json.template` - существует
- [ ] `bryx CRM/bryx CRM/appsettings.json` - **НЕ** должен быть в git (проверьте .gitignore)

### ✅ Файлы Bot проекта

- [ ] `bryxBot/Dockerfile` - существует
- [ ] `bryxBot/BryxBot/appsettings.Production.json` - существует (БЕЗ секретов)
- [ ] `bryxBot/BryxBot/appsettings.Development.json.template` - существует
- [ ] `bryxBot/BryxBot/appsettings.json` - **НЕ** должен быть в git (проверьте .gitignore)

## 🧪 Локальное тестирование

### Перед пушем в GitHub

- [ ] CRM запускается локально без ошибок
- [ ] Bot запускается локально без ошибок
- [ ] CRM подключается к локальной PostgreSQL
- [ ] Bot может подключиться к CRM API
- [ ] Миграции применяются успешно
- [ ] Нет критических ошибок в логах

### Команды для проверки

```bash
# Проверка CRM
cd "bryx CRM/bryx CRM"
dotnet build
dotnet run

# В другом терминале - проверка Bot
cd bryxBot/BryxBot
dotnet build
dotnet run
```

## 📦 Подготовка к Git

### 1. Проверка статуса

```bash
cd bryx_base
git status
```

### 2. Просмотр изменений

```bash
git diff
```

### 3. Проверка .gitignore

```bash
# Эта команда покажет, какие файлы будут игнорироваться
git check-ignore -v *
```

### 4. Сухая проверка (без реального добавления)

```bash
git add -A --dry-run
```

**Проверьте вывод!** Если видите файлы с секретами - **ОСТАНОВИТЕСЬ** и обновите `.gitignore`

### 5. Если все ОК, добавьте файлы

```bash
git add .
git commit -m "Initial commit: Bryx CRM + Bot ready for Railway"
```

## 🚀 Пуш на GitHub

### 1. Добавьте удаленный репозиторий

```bash
git remote add origin https://github.com/YOUR_USERNAME/bryx-crm.git
```

### 2. Проверьте ветку

```bash
git branch -M main
```

### 3. Пуш

```bash
git push -u origin main
```

### 4. Проверьте GitHub

- [ ] Откройте репозиторий на GitHub
- [ ] Убедитесь, что **НЕТ** файлов `appsettings.json` с токенами
- [ ] Убедитесь, что **ЕСТЬ** файлы `appsettings.Production.json`
- [ ] Убедитесь, что **ЕСТЬ** `.env.example`

## ⚙️ Подготовка к Railway

### Сохраните эти значения (понадобятся при деплое):

```
TELEGRAM_BOT_TOKEN=___________________________
TELEGRAM_CHAT_ID=___________________________
ALLOWED_USERS=___________________________
```

### Для локальной разработки:

Скопируйте `.env.example` в `.env` и заполните:

```bash
cp .env.example .env
```

Отредактируйте `.env` своими значениями.

**⚠️ ВАЖНО**: `.env` не должен быть в Git! Проверьте, что он в `.gitignore`!

## 🎯 Готовность к деплою

### Финальная проверка

- [ ] Все файлы закоммичены и запушены на GitHub
- [ ] В репозитории НЕТ секретных данных
- [ ] Сохранены все токены и ID в безопасном месте
- [ ] Прочитана документация `DEPLOYMENT.md`
- [ ] Railway аккаунт создан и готов

### 🎉 Вы готовы к деплою!

Следующий шаг: откройте `DEPLOYMENT.md` и следуйте инструкциям.

---

## ❓ Возникли проблемы?

### "Git добавляет файлы с секретами"

**Решение:**
1. Удалите файлы из staging area:
   ```bash
   git reset HEAD appsettings.json
   git reset HEAD .env
   ```
2. Обновите `.gitignore`
3. Проверьте снова: `git status`

### "Забыл удалить секреты перед коммитом"

**Решение:**
1. **НЕ** пушьте в GitHub!
2. Отмените последний коммит:
   ```bash
   git reset --soft HEAD~1
   ```
3. Удалите файлы с секретами из staging
4. Обновите `.gitignore`
5. Сделайте новый коммит

### "Случайно запушил секреты на GitHub"

**КРИТИЧНО! Действуйте быстро:**
1. **Немедленно** измените все токены и пароли:
   - Создайте новый Telegram Bot Token через @BotFather
   - Смените пароль PostgreSQL
2. Удалите историю коммитов или сделайте репозиторий приватным
3. Force push с очищенной историей (осторожно!)

---

**Удачного деплоя! 🚀**
