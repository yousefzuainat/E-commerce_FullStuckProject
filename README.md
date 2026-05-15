# 🛒 VOLTEX - E-Commerce Web Application

<div align="center">
  <img src="https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core" />
  <img src="https://img.shields.io/badge/Entity_Framework-0078D4?style=for-the-badge&logo=.net&logoColor=white" alt="Entity Framework" />
  <img src="https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=for-the-badge&logo=tailwind-css&logoColor=white" alt="Tailwind CSS" />
  <img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black" alt="JavaScript" />
  <img src="https://img.shields.io/badge/LINQ-00599C?style=for-the-badge&logo=c-sharp&logoColor=white" alt="LINQ" />
</div>

<br/>

## 📌 Project Overview
**VOLTEX** is a full-featured, modern E-Commerce Web Application built using **ASP.NET Core MVC**. It provides a robust architecture with complete user and admin roles, secure authentication, dynamic product and category management, a persistent shopping cart, an advanced order system, and an interactive, highly responsive UI powered by modern web technologies.

---

## 🛠️ Technologies & Stack
* **Backend:** C#, ASP.NET Core MVC, LINQ
* **Database:** Microsoft SQL Server, Entity Framework Core (Code-First Approach)
* **Frontend:** HTML5, CSS3, Tailwind CSS (Admin Dashboard), Bootstrap, JavaScript, jQuery
* **Authentication:** ASP.NET Core Identity (Role-based Authorization)
* **Notifications:** SweetAlert2 & Toastr for real-time user feedback
* **Payment Gateway:** Stripe / PayPal External API Integration

---

## 👥 User Roles & Permissions

### 👑 Admin (System Management)
The Admin has full control over the system through a premium, dark-themed dashboard:
- ➕ Add / ✏️ Edit / 👁️ View / ❌ Delete **Categories** & **Products**
- 🖼️ Manage multiple images per product (Strict image-only uploads).
- 👥 View and manage registered users.
- 📊 **Dashboard Statistics:** Live tracking of total users, products, categories, and orders.
- 📦 **Stock Management:** Real-time product inventory control.
- 📑 **Order Management:**
  - View all global orders or filter by specific user.
  - Dynamically update order statuses (e.g., Pending, Processing, Paid, Completed, Cancelled).
- 🔍 **Advanced Filtering:** Easily search through Categories, Products, Comments, Orders, and Users.
- ✅ **Moderation:** Approve or Reject user Testimonials and Product Comments/Ratings.

### 🙋‍♂️ User (Customer Experience)
Customers enjoy a seamless, highly engaging shopping experience:
- 🛍️ Browse products and categories with a clean UI.
- 🔍 **Advanced Search:** Search by product or category name with partial text support.
- 🛒 **Persistent Shopping Cart:** 
  - Add products without logging in (Powered by secure Browser Cookies).
  - Data remains safely stored even if the browser is closed or the project is restarted!
  - Quantity-based system (no duplicate line items, dynamic total calculation).
- ❤️ **Wishlist:** Save favorite products for quick access later.
- 💬 Add comments and submit **⭐ Ratings** on products.
- 📝 Submit platform testimonials (featured on Home & About pages after admin approval).
- 📦 **Order Tracking:**
  - View current and previous order history.
  - View full details (Products, Quantities, Pricing, Savings).
  - 🧾 **Digital Invoice** generation for every order.
- 💳 **Secure Checkout:** Login enforced at checkout, connecting to a secure payment API.

---

## 🔐 Authentication & Security
- 🔑 Secure Login & Registration pages.
- 🛡️ Powered by **ASP.NET Identity** with secure password hashing.
- 🚧 **Role-based Authorization:** Admin pages and API endpoints are strictly protected. Users must be authenticated to finalize checkouts.

---

## 🎨 UI / UX Highlights
- 💎 **Premium Dark Mode Admin Dashboard:** Built with custom Tailwind CSS for a stunning administrative experience.
- 🔔 **Interactive Feedback:** Uses SweetAlert2 / Toast notifications for all actions (Add to cart, Login, Checkout success, Order status updates).
- 📱 Fully responsive design adapting beautifully to Desktop, Tablet, and Mobile devices.

---

## 🚀 How to Run the Project
1. Clone the repository.
2. Open the solution `ecommerce system.slnx` in Visual Studio.
3. Open `appsettings.json` and verify your `DefaultConnection` string for SQL Server.
4. Open the Package Manager Console and run:
   ```bash
   Update-Database
   ```
5. Run the project (F5).
6. **Default Admin Account:**
   - **Email:** Admin@gmail.com
   - **Password:** Admin@123

---
> *Built with ❤️ focusing on clean MVC architecture, scalability, and an exceptional user experience.*
