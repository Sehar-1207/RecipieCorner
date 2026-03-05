<h1 align="center">  Recipe Corner & Food Secrets </h1>
<p align="center"> A Comprehensive Enterprise-Grade Culinary Management Ecosystem & Social Recipe Platform </p>

<p align="center">
  <img alt="Build" src="https://img.shields.io/badge/Build-Passing-brightgreen?style=for-the-badge">
  <img alt="Framework" src="https://img.shields.io/badge/Framework-.NET%20Core-512bd4?style=for-the-badge">
  <img alt="Architecture" src="https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=for-the-badge">
  <img alt="Contributions" src="https://img.shields.io/badge/Contributions-Welcome-orange?style=for-the-badge">
  <img alt="License" src="https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge">
</p>

<!-- 
  **Note:** These are static placeholder badges. Replace them with your project's actual badges.
  You can generate your own at https://shields.io
-->

---

## 📑 Table of Contents
- [🔍 Overview](#-overview)
- [✨ Key Features](#-key-features)
- [🛠️ Tech Stack & Architecture](#-tech-stack--architecture)
- [📁 Project Structure](#-project-structure)
- [🚀 Getting Started](#-getting-started)
- [🔧 Usage](#-usage)
- [🤝 Contributing](#-contributing)

---

## 🔍 Overview

**Recipe Corner** is a sophisticated, dual-layered digital ecosystem designed to bridge the gap between complex culinary data management and a seamless user-facing experience. At its core, it provides a robust API-driven backend for managing the intricate relationships between recipes, ingredients, instructions, and user sentiments, while the **Food Secrets** frontend provides a high-performance, responsive web interface for enthusiasts to discover and share culinary masterpieces.

> In the modern culinary world, organizing recipes, tracking precise ingredient metrics, and maintaining step-by-step instructional integrity is a significant challenge for both home cooks and professional chefs. Data fragmentation often leads to lost secrets and inconsistent results. RecipeCorner solves this by centralizing the culinary lifecycle—from raw ingredient definitions to community-driven ratings—into a unified, secure, and highly scalable platform.

The system utilizes an **N-Tier Architecture** consisting of a dedicated Web API project (`RecipieCorner`) for data persistence and business logic, and a refined MVC Web Application (`FoodSecrets`) for the user interface. By decoupling the data service from the presentation layer, the ecosystem ensures maximum flexibility, security, and future-proof scalability.

---

## ✨ Key Features

### 🔐 Enterprise-Grade Security & Authentication
*   **Secure Identity Management:** Utilizes ASP.NET Identity and JWT (JSON Web Tokens) for stateless, secure communication between the frontend and the API.
*   **Granular Profile Control:** Users can manage their identity through dedicated `UpdateProfile` models and secure `AuthAccount` controllers.
*   **Token-Based Persistence:** Implements `JwtTokenService` to handle authentication handshakes, ensuring user sessions are both secure and lightweight.

### 📖 Precision Recipe Management
*   **Structured Data Modeling:** Recipes are more than just text; they are complex entities linked to specific ingredients, instructional steps, and user ratings.
*   **Dynamic Image Hosting:** Supports a robust image system for both food and user avatars, stored efficiently in the `wwwroot/Images` directory.
*   **Detailed Metadata:** Every recipe tracks essential details, supported by `RecipeDto` and `RecipeDetailsDto` for optimized data transfer.

### 🧂 Intelligent Ingredient & Step Tracking
*   **Granular Ingredient Control:** Manage individual ingredients with specific properties, ensuring that every component of a dish is accounted for.
*   **Step-by-Step Guidance:** The `Instruction` module breaks down complex culinary processes into manageable, sequential steps, improving user success rates in the kitchen.

### ⭐ Social Engagement & Feedback Loop
*   **Community Ratings:** A dedicated `Rating` system allows users to provide feedback on recipes, fostering a community of shared culinary expertise.
*   **Interactive UI Components:** Features like `Rating.js` and dedicated `RatingUiController` provide a real-time, interactive experience for user feedback.

### 🎨 High-Performance Frontend (FoodSecrets)
*   **Responsive Design:** Utilizing a mix of Bootstrap and custom CSS (e.g., `Dashboard.css`, `RecipeIndex.css`), the platform offers a "mobile-first" experience.
*   **Dynamic Views:** Sophisticated CSHTML layouts including `_DashboardLayout` and `_RecipeCardsPartial` ensure a consistent and professional aesthetic across all modules.

---

## 🛠️ Tech Stack & Architecture

The project is built using a modern .NET ecosystem, emphasizing the Repository and Unit of Work patterns to ensure clean, maintainable, and testable code.

| Technology | Purpose | Why it was Chosen |
| :--- | :--- | :--- |
| **ASP.NET Core API** | Backend Services | Provides high-performance, cross-platform RESTful endpoints for the core business logic. |
| **ASP.NET Core MVC** | Web Frontend | Enables a powerful Model-View-Controller pattern for the `FoodSecrets` user interface. |
| **Entity Framework Core** | ORM | Simplifies data access with a code-first approach, managed via Migrations (as seen in `Migrations/`). |
| **JWT Authentication** | Security | Ensures secure, stateless communication between the MVC app and the API. |
| **Repository Pattern** | Data Abstraction | Decouples the business logic from data access code, using `IGeneric` and `IUnitOfWork` interfaces. |
| **SQL Server** | Database | Provides enterprise-level relational data storage for complex recipe and user relationships. |
| **Bootstrap & jQuery** | Styling & UI | Industry-standard libraries for responsive design and client-side interactivity. |

---

## 📁 Project Structure

```
Sehar-1207-RecipieCorner-d7a6629/
├── 📄 RecipeCorner.sln             # Visual Studio Solution File
├── 📄 .gitignore                   # Git exclusion rules
├── 📂 RecipieCorner/               # CORE API & DATA PROJECT
│   ├── 📄 Program.cs               # API Entry Point
│   ├── 📂 Controllers/             # API Endpoints (Auth, Ingredient, Instruction, Rating, Recipe)
│   ├── 📂 Data/                    # DbContext & Database configuration
│   ├── 📂 Dtos/                    # Data Transfer Objects for optimized API responses
│   ├── 📂 Interfaces/              # Abstraction layer (IUnitOfWork, IGeneric)
│   ├── 📂 Migrations/              # Entity Framework database snapshots
│   ├── 📂 Models/                  # Core Business Entities (Recipe, Ingredient, Rating, User)
│   ├── 📂 Repositories/            # Implementation of the Repository Pattern
│   └── 📂 Services/                # Logic services (JwtTokenService)
├── 📂 FoodSecrets/                 # FRONTEND MVC PROJECT
│   ├── 📄 Program.cs               # Web App Entry Point
│   ├── 📂 Controllers/             # UI Logic (AuthAccount, IngredientUi, RecipeUi, etc.)
│   ├── 📂 Services/                # API Client services (AuthAccountService, RecipeService)
│   ├── 📂 Views/                   # Razor Views (Home, Auth, Recipe management)
│   │   ├── 📂 Shared/              # Reusable Layouts (Dashboard, User, Login layouts)
│   └── 📂 wwwroot/                 # Static Assets
│       ├── 📂 css/                 # Custom styles (Dashboard.css, UserRecipeDetail.css)
│       ├── 📂 js/                  # Client-side logic (Rating.js, site.js)
│       └── 📂 Images/              # Uploaded content (Food and User avatars)
```

---

## 🚀 Getting Started

### Prerequisites
*   **.NET 8.0 SDK** or later
*   **SQL Server** (LocalDB or Express)
*   **Visual Studio 2022** (recommended) or VS Code

### Installation Steps

1.  **Clone the Repository**
    ```bash
    git clone https://github.com/Sehar-1207/RecipieCorner.git
    cd RecipieCorner
    ```

2.  **Database Configuration**
    Update the `ConnectionStrings` in both `RecipieCorner/appsettings.json` and `FoodSecrets/appsettings.json` to point to your local SQL Server instance.

3.  **Apply Migrations**
    Open your terminal in the `RecipieCorner` directory and run:
    ```bash
    dotnet ef database update
    ```

4.  **Run the Backend API**
    Navigate to the `RecipieCorner` folder:
    ```bash
    dotnet run
    ```

5.  **Run the Frontend MVC**
    Open a new terminal, navigate to the `FoodSecrets` folder:
    ```bash
    dotnet run
    ```

---

## 🔧 Usage

### For Developers: Interacting with the API
The `RecipieCorner` API provides several endpoints for data manipulation. You can explore these via the Swagger UI (usually available at `/swagger` when running in development).

*   **Auth:** `POST /api/Auth/login` and `POST /api/Auth/register`
*   **Recipes:** `GET /api/Recipe` to fetch all culinary entries.
*   **Ingredients:** Managed through the `IngredientController`.

### For Users: Exploring FoodSecrets
1.  **Dashboard:** Upon logging in, users are greeted with a personalized dashboard (utilizing `_DashboardLayout.cshtml`).
2.  **Recipe Discovery:** Browse recipes on the Index page, featuring dynamic cards (`_RecipeCardsPartial.cshtml`).
3.  **Detailed View:** Click on any recipe to see full `Details.cshtml`, including high-quality images from the `wwwroot/Images/food` directory.
4.  **Interactive Steps:** Follow sequential instructions to recreate dishes perfectly.

---

## 🤝 Contributing

I welcome contributions to improve RecipeCorner! Whether you're fixing a bug in the `GenericRepo` or enhancing the `RecipeIndex.css` styling, your help is appreciated.

### How to Contribute

1. **Fork the repository** - Click the 'Fork' button at the top right of this page.
2. **Create a feature branch** 
   ```bash
   git checkout -b feature/culinary-improvement
   ```
3. **Make your changes** - Ensure code consistency with the existing Repository pattern.
4. **Test thoroughly** - Verify that both the API and MVC project communicate correctly.
5. **Commit your changes** 
   ```bash
   git commit -m 'Add: New feature for ingredient scaling'
   ```
6. **Push to your branch**
   ```bash
   git push origin feature/culinary-improvement
   ```
7. **Open a Pull Request** - Describe your changes in detail.

### Development Guidelines
*   ✅ Follow C# naming conventions and clean code principles.
*   📝 Maintain the DTO pattern for all data transfers.
*   🧪 Ensure all new models are properly migrated.
*   🎨 Keep the UI responsive and compatible with existing CSS frameworks.

---

### What this means:
*   ✅ **Commercial use:** You can use this project commercially.
*   ✅ **Modification:** You can modify the code freely.
*   ✅ **Distribution:** You can distribute this software to others.
*   ✅ **Private use:** You can use this project privately for your own culinary needs.
*   ⚠️ **Liability:** The software is provided "as is", without warranty of any kind.

---

<p align="center">Made with ❤️ by the Sehar Ajmal.</p>
