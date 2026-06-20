# TODO App — веб-додаток для управління завданнями

Full-stack застосунок у стилі Microsoft To Do: реєстрація, JWT-авторизація, категорії, завдання з дедлайнами, вкладки списків, пошук і двомовний інтерфейс (UK/EN).



---

## Зміст

1. [Мета проєкту](#мета-проєкту)
2. [Архітектура](#архітектура)
3. [Структура рішення](#структура-рішення)
4. [Модель даних](#модель-даних)
5. [Як працює API](#як-працює-api)
6. [Бізнес-логіка вкладок](#бізнес-логіка-вкладок)
7. [Пошук і фільтри](#пошук-і-фільтри)
8. [Frontend (Angular)](#frontend-angular)
9. [Авторизація JWT](#авторизація-jwt)
10. [Скріншоти для доповіді](#скріншоти-для-доповіді)
11. [Запуск проєкту](#запуск-проєкту)
12. [Тестування](#тестування)
13. [Шпаргалка для захисту](#шпаргалка-для-захисту)

---

## Мета проєкту

Створити персональний todo-додаток, де кожен користувач:

- реєструється та входить через JWT;
- створює **категорії** (Дім, Робота, Продукти…);
- додає **завдання** з описом, дедлайном, позначкою «важливо»;
- переглядає завдання у **розумних списках** (Мій день, Заплановано, Виконано…);
- **шукає** за текстом і **фільтрує** за категорією та діапазоном дат;
- перемикає **мову інтерфейсу** (українська / англійська).

---

## Архітектура

Застосунок побудований за **шаровою (layered) архітектурою** — кожен шар має одну відповідальність.

```mermaid
flowchart TB
    subgraph Client["Angular Client :4200"]
        UI[Components + Services]
        Guard[Auth Guards]
        I18n[i18n + LanguageService]
    end

    subgraph API["ASP.NET Core API :5000"]
        Ctrl[Controllers]
        Svc[Services + Validators]
        MW[Exception Middleware]
    end

    subgraph Data["Data Access"]
        Repo[Repositories]
        EF[EF Core + AppDbContext]
    end

    subgraph DB["PostgreSQL"]
        Tables[(Users · Categories · Tasks)]
    end

    UI -->|HTTP + JWT| Ctrl
    Ctrl --> Svc
    Svc --> Repo
    Repo --> EF
    EF --> Tables
```

### Потік запиту (приклад: завантаження завдань)

1. Користувач відкриває вкладку **«Мій день»** → Angular викликає `TaskService.getTasks(..., listType: 'myday')`.
2. HTTP GET `/api/tasks?listType=myday&pageNumber=1&pageSize=10` з заголовком `Authorization: Bearer <token>`.
3. `TasksController` отримує `userId` з JWT і викликає `TaskService`.
4. `TaskService` делегує в `TaskRepository`, де формується LINQ-запит до PostgreSQL.
5. Результат повертається як `PagedResult<TaskDto>` у JSON (camelCase).

---

## Структура рішення

| Проєкт | Призначення |
|--------|-------------|
| `TODO-App.Api` | HTTP API, JWT, Swagger, CORS, middleware |
| `TODO-App.Services` | Бізнес-логіка, DTO, FluentValidation |
| `TODO-App.DataAccess` | EF Core, репозиторії, міграції |
| `TODO-App.Domain` | Сутності, інтерфейси репозиторіїв |
| `TODO-App.Client` | Angular SPA (компоненти, сервіси, i18n) |
| `TODO-App.Tests` | Unit- та integration-тести (xUnit) |

---

## Модель даних

```mermaid
erDiagram
    User ||--o{ Category : owns
    User ||--o{ ToDoTask : owns
    Category ||--o{ ToDoTask : groups

    User {
        int Id PK
        string Username
        string PasswordHash
    }

    Category {
        int Id PK
        string Name
        int UserId FK
    }

    ToDoTask {
        int Id PK
        string Title
        string Description
        bool IsCompleted
        bool IsImportant
        datetime Deadline
        datetime CompletedAt
        datetime CreatedAt
        int UserId FK
        int CategoryId FK
    }
```

### Ключові поля завдання

| Поле | Навіщо |
|------|--------|
| `Deadline` | Дедлайн — для вкладок «Мій день», «Заплановано», фільтра дат |
| `IsImportant` | Зірочка — вкладка «Важливо» |
| `IsCompleted` / `CompletedAt` | Статус виконання — вкладка «Виконано» |
| `CreatedAt` | Дата створення — «Мій день» для завдань без дедлайну |
| `CategoryId` | Зв'язок із категорією користувача |

---

## Як працює API

### Auth

| Метод | URL | Опис |
|-------|-----|------|
| POST | `/api/auth/register` | Реєстрація → JWT |
| POST | `/api/auth/login` | Вхід → JWT |
| POST | `/api/auth/logout` | Logout (клієнт видаляє токен) |

### Categories (потрібен JWT)

| Метод | URL | Опис |
|-------|-----|------|
| GET | `/api/categories` | Список категорій поточного користувача |
| POST | `/api/categories` | Створити категорію |
| PUT | `/api/categories/{id}` | Оновити |
| DELETE | `/api/categories/{id}` | Видалити |

### Tasks (потрібен JWT)

| Метод | URL | Опис |
|-------|-----|------|
| GET | `/api/tasks` | Список з пагінацією, фільтрами, `listType` |
| GET | `/api/tasks/{id}` | Одне завдання |
| POST | `/api/tasks` | Створити |
| PUT | `/api/tasks/{id}` | Оновити |
| DELETE | `/api/tasks/{id}` | Видалити |

### Query-параметри GET `/api/tasks`

| Параметр | Приклад | Опис |
|----------|---------|------|
| `pageNumber` | `1` | Номер сторінки |
| `pageSize` | `10` | Розмір сторінки |
| `categoryId` | `2` | Фільтр за категорією |
| `search` | `молоко` | Пошук у назві/описі (без урахування регістру) |
| `listType` | `myday` | Тип списку (див. нижче) |
| `dateFrom` | `2026-06-15` | Дедлайн від (формат `yyyy-MM-dd`) |
| `dateTo` | `2026-06-20` | Дедлайн до (включно) |

---

## Бізнес-логіка вкладок

Логіка реалізована в `TaskRepository` — **на бекенді**, щоб фронтенд лише передавав `listType`.

| Вкладка | `listType` | Умова |
|---------|------------|-------|
| **Мій день** | `myday` | Невиконані + (прострочені **або** дедлайн сьогодні **або** створені сьогодні без дедлайну) |
| **Важливо** | `important` | `IsImportant = true` і не виконані |
| **Заплановано** | `planned` | Є дедлайн, не виконані, дедлайн ≥ сьогодні |
| **Виконано** | `completed` | `IsCompleted = true` |
| **Призначено мені** | `assignedtome` | Усі невиконані |
| **Завдання** | `tasks` | Усі завдання без обмеження за статусом |

### «Мій день» — три групи завдань

```
1. Прострочені:     Deadline < сьогодні, IsCompleted = false
2. На сьогодні:     Deadline у межах сьогоднішнього дня
3. Без дедлайну:    CreatedAt сьогодні, Deadline = null
```

**Сортування:** спочатку прострочені → потім за дедлайном.

### «Заплановано» — календарний огляд

```
Deadline != null
IsCompleted = false
Deadline >= початок сьогоднішнього дня
```

**Сортування:** `OrderBy(Deadline)` — найближчі зверху.

> Прострочені завдання потрапляють у **«Мій день»**, а не у **«Заплановано»** — це навмисне розділення «боргу» і «календаря».

---

## Пошук і фільтри

### Текстовий пошук

- Регістронезалежний (`ToLower().Contains()`).
- На фронтенді — **миттєвий** (debounce 250 ms), без кнопки «Застосувати».
- При помилці API список **очищується**, щоб не показувати застарілі дані.

### Фільтр за датами

- Фільтрує за полем **Deadline**.
- Дати передаються як рядки `yyyy-MM-dd` і парсяться через `TaskQueryDateParser`.
- Якщо `dateFrom > dateTo` — дати автоматично міняються місцями.

### Прострочені дедлайни (UI)

- Невиконані завдання з `Deadline < now` підкреслюються **червоним** (`task-overdue` CSS-клас).

---

## Frontend (Angular)

### Маршрути

| URL | Компонент | Доступ |
|-----|-----------|--------|
| `/login` | LoginComponent | тільки гості |
| `/register` | RegisterComponent | тільки гості |
| `/tasks` | TasksComponent | авторизовані |
| `/categories` | CategoriesComponent | авторизовані |
| `/settings` | SettingsComponent | авторизовані |

### Guards

- `authGuard` — не пускає без JWT.
- `guestGuard` — не пускає авторизованих на login/register.

### i18n

- Файл перекладів: `TODO-App.Client/src/app/i18n/translations.ts`
- `LanguageService` зберігає мову в `localStorage`
- Pipe `| t` для шаблонів
- Перемикач мови: `LanguageSwitcherComponent` + сторінка Settings

### Ключові сервіси

| Сервіс | Роль |
|--------|------|
| `AuthService` | login/register, збереження JWT |
| `TaskService` | CRUD завдань, query-параметри |
| `CategoryService` | CRUD категорій |
| `LanguageService` | поточна мова, translate() |

---

## Авторизація JWT

```mermaid
sequenceDiagram
    participant U as Користувач
    participant A as Angular
    participant API as .NET API
    participant DB as PostgreSQL

    U->>A: login / password
    A->>API: POST /api/auth/login
    API->>DB: перевірка PasswordHash
    API-->>A: { token, username, userId }
    A->>A: localStorage.setItem('token')
    A->>API: GET /api/tasks + Authorization: Bearer
    API->>API: валідація JWT, userId з claims
    API-->>A: список завдань користувача
```

- Паролі зберігаються як **хеш** (не plain text).
- Кожен запит до tasks/categories ізолює дані за `UserId`.
- JWT — **stateless**: сервер не зберігає сесії; logout = видалення токена на клієнті.

---

## Скріншоти для доповіді

Створіть папку `docs/screenshots/` і додайте зображення. У README залишені placeholder-и — замініть шляхи після вставки.

### 1. Вхід / реєстрація

<img width="670" height="568" alt="image" src="https://github.com/user-attachments/assets/ccb65a9c-eda4-408c-8937-8f50f191318f" />

> 📸 *Скріншот: `/login` — форма входу з локалізованими підписами.*

<img width="660" height="561" alt="image" src="https://github.com/user-attachments/assets/8f3ca3a5-d862-41ff-8748-29ab0e37e25e" />

> 📸 *Скріншот: `/register` — створення нового акаунта.*

---

### 2. Головна — вкладки завдань

<img width="2557" height="1236" alt="image" src="https://github.com/user-attachments/assets/b92d65de-ada5-40a4-b3e9-966dd19dcebe" />

> 📸 *Скріншот: вкладки «Мій день», «Важливо», «Заплановано», «Виконано»…*

<img width="1003" height="1051" alt="image" src="https://github.com/user-attachments/assets/489c2cd1-d4d9-4ccd-abdd-4ea7a0de3887" />

> 📸 *Скріншот: «Мій день» — прострочені + сьогоднішні завдання.*

<img width="1009" height="1150" alt="image" src="https://github.com/user-attachments/assets/605e01a2-92fc-4f31-ab32-fada2c8e143a" />

> 📸 *Скріншот: «Заплановано» — майбутні завдання, відсортовані за дедлайном.*

---

### 3. Пошук і фільтри

<img width="1495" height="1014" alt="image" src="https://github.com/user-attachments/assets/cda46a40-b9f1-41e4-aefa-a9c168464cc9" />

> 📸 *Скріншот: миттєвий пошук під час введення тексту.*

<img width="1531" height="853" alt="image" src="https://github.com/user-attachments/assets/e97a22d2-c25b-469b-b89e-1402b1040124" />

> 📸 *Скріншот: поля «Дата від» / «Дата до» + результат фільтрації.*

<img width="1480" height="748" alt="image" src="https://github.com/user-attachments/assets/335944b9-34b6-4694-855a-022d37d7dde7" />

> 📸 *Скріншот: червоне підкреслення прострочених дедлайнів.*

---

### 4. Категорії та налаштування

<img width="1557" height="544" alt="image" src="https://github.com/user-attachments/assets/836b7164-50a6-49c0-ad9b-2fdb62115185" />

> 📸 *Скріншот: сторінка `/categories`.*

<img width="1504" height="346" alt="image" src="https://github.com/user-attachments/assets/b2790ec3-5dcc-4062-9faa-f9d5712a138a" />
<img width="1513" height="352" alt="image" src="https://github.com/user-attachments/assets/8d8964c8-b23e-49a6-bdf8-8b0fc9408214" />


> 📸 *Скріншот: `/settings` — перемикач UK/EN.*

---

### 5. Backend

<img width="1120" height="1159" alt="image" src="https://github.com/user-attachments/assets/996e4099-bc15-4a73-9be0-659c23ca3245" />

> 📸 *Скріншот: Swagger UI `http://localhost:5000/swagger`.*

<img width="537" height="190" alt="image" src="https://github.com/user-attachments/assets/530fa08c-7782-4f97-b714-9d1027af8b50" />

> 📸 *Скріншот: pgAdmin / таблиці Users, Categories, Tasks.*

---

## Запуск проєкту

### Вимоги

- .NET 8 SDK
- Node.js 18+
- PostgreSQL 15+
- Angular CLI (`npm install -g @angular/cli`)

### 1. База даних

Налаштуйте `TODO-App.Api/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=TodoDb;Username=postgres;Password=YOUR_PASSWORD"
}
```

Застосуйте міграції:

```powershell
dotnet ef database update `
  --project TODO-App.DataAccess `
  --startup-project TODO-App.Api
```

### 2. Backend

```powershell
cd D:\RiderProjects\TODO-App
dotnet run --project TODO-App.Api
```

API: `http://localhost:5000`  
Swagger: `http://localhost:5000/swagger`

### 3. Frontend

```powershell
cd TODO-App.Client
npm install
npm start
```

UI: `http://localhost:4200`

---

## Тестування

```powershell
dotnet test TODO-App.Tests
```

Покриття включає:

- CRUD завдань, пагінацію, пошук;
- фільтри `listType` (My Day, Planned, Important…);
- фільтр діапазону дат;
- валідацію формату дати (400 Bad Request);
- unit-тести сервісного шару та парсера дат.

---


## Автор

**Тюпа Максим** 

---

*Оновлено для гілки `feature/task-lists-search-i18n`.*
