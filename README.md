🍽️ LocalFood — це сучасна веб-платформа для замовлення їжі, яка об'єднує локальні ресторани, кур'єрів та клієнтів в єдиній інтерактивній системі. Проєкт реалізовано на основі ASP.NET Core MVC, Entity Framework Core, MS SQL Server і сучасних вебтехнологій з акцентом на безпеку, ефективність та стильний UI.
---

🚀 Основні можливості
👨‍🍳 Адміністратор:

Керує стравами, ресторанами, користувачами та статусами замовлень.

Створює меню та аналітику замовлень.

Призначає ролі (Admin, Courier, User).

🍕 Користувач:

Переглядає список ресторанів та меню.

Формує замовлення через кошик.

Відстежує статус доставки в реальному часі.

Може редагувати профіль, змінювати пароль та відновлювати його через email.

🚴‍♂️ Кур'єр:

Переглядає всі активні замовлення.

Бере замовлення в доставку, змінює статус на "В дорозі" або "Доставлено".

Бачить маршрут доставки від ресторану до клієнта (побудова на мапі).
---

🔒 Безпека
✅ Авторизація через ASP.NET Identity.

✅ Підтвердження email та відновлення паролю через SMTP.

✅ Захист від CSRF, XSS.

✅ Підтримка ролей: Admin, User, Courier.

💡 Технології
🧠 ASP.NET Core MVC

💾 Entity Framework Core + MS SQL Server

📧 SMTP Email Sender (через Gmail або інший сервер)

🌐 Bootstrap 5 + Font Awesome + Canvas анімації

🗺️ Інтеграція мап для доставки (Mapbox / Leaflet)

---

## ⚙️ Технології

| Сфера       | Технології                              |
|-------------|------------------------------------------|
| Backend     | ASP.NET Core MVC, Entity Framework Core |
| Frontend    | Bootstrap 5, Font Awesome, Canvas       |
| Database    | SQL Server (LocalDB), Code First EF     |
| Email       | SMTP (Gmail, будь-який інший сервер)    |
| Безпека     | ASP.NET Identity (CSRF, XSS захист)     |
| Мапи        | Leaflet, OpenStreetMap                  |

---
## 🗂️ Архітектура проєкту

- 🔐 `AccountController` – реєстрація, вхід, скидання паролю, email-підтвердження
- 📦 `CartController` – додавання страв у кошик, оформлення замовлення
- 🚚 `CourierController` – кур'єр бачить і бере доставку, змінює статус, бачить маршрут
- 🧾 `OrdersController` – створення, редагування, перегляд замовлень
- 🥗 `DishesController` – CRUD для меню
- 🏠 `HomeController` – головна сторінка
- 🏢 `RestaurantsController` – управління ресторанами
- 👤 `ProfileController` – редагування профілю користувача
- 📊 Аналітика – (опційно) порівняння страв за категоріями, цінами, роками

---

## 🔐 Безпечна автентифікація та email

- 🔑 Ролі: **Admin**, **User**, **Courier**
- 📨 Відновлення паролю з email-токеном
- 💬 Повідомлення надсилаються через SMTP (Gmail або інший постачальник)
- 🔐 Захист: CSRF, XSS, HTTPS, strong-password policy

---
👩‍💻 Авторка
Розроблено з ❤️ повністю власноруч як навчально-прикладний проєкт.
Усе — від бази даних до front-end анімацій та відправки email.
Готовий для масштабування або підключення до реального закладу 🍱


