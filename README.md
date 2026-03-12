# InventoryApp

A full-featured web application for inventory management built with **C# / Blazor Server / .NET 8** and **PostgreSQL**. Users can create custom inventories, define custom fields, manage items with auto-generated IDs, and collaborate with other users in real time.

🌐 **Live Demo:** https://inventoryapp-wn95.onrender.com
---

## Features

### Core
- **Social Authentication** — Login with Google or GitHub
- **Role System** — User and Admin roles; admins have full access to all inventories
- **Responsive Design** — Works on desktop and mobile
- **Light / Dark Theme** — User preference saved via cookie
- **EN / RU Language Support** — UI fully translated, user preference saved via cookie
- **Full-Text Search** — Search across inventories and items from the header

### Inventories
- Create inventories with name, description, category, image, and tags
- **Auto-save** every 8 seconds with optimistic locking (version-based conflict detection)
- **Tag autocomplete** — dropdown suggests existing tags as you type
- **Tag cloud** on the home page — clickable tags filter inventories
- Inventory cover image uploaded to **Cloudinary**
- Markdown rendering for descriptions

### Items
- Table view with clickable rows (no buttons in rows)
- Custom fields shown as extra columns in the table (configurable per field)
- Add, edit, delete items (for users with write access)
- **Like system** — one like per user per item

### Custom ID Engine
- Define a custom ID format per inventory using composable parts:
  - Fixed Text, 6/9-digit Random, 20/32-bit Random, GUID, Date/Time, Sequence
- **Drag-and-drop** reordering of ID parts
- Real-time preview of the generated ID
- IDs are auto-generated on item creation
- Custom IDs are editable on the item form
- Uniqueness enforced at database level (composite index on `inventory_id + custom_id`)

### Custom Fields
- Up to 3 fields per type: Single Line, Multi Line, Numeric, Document Link, Checkbox
- Each field has a title, description (tooltip), and "show in table" toggle
- **Drag-and-drop** reordering of fields
- EAV (Entity-Attribute-Value) storage pattern

### Access Control
- Mark inventory as **public** (all authenticated users can add items)
- Or manually grant write access to specific users
- User search with autocomplete by email

### Discussion
- Linear comment thread per inventory
- **Real-time updates** via 3-second polling
- Markdown rendering in posts
- Username links to public profile pages

### Statistics Tab
- Total items, total likes, total comments
- Numeric field stats: min, max, average
- Text field stats: most frequently used values

### Admin Panel
- Checkbox-based bulk selection (select all / deselect all)
- Bulk actions: Block, Unblock, Make Admin, Remove Admin, Delete
- Self-protection — cannot block or delete your own account

### Main Page
- Latest inventories
- Top 5 inventories by item count
- Tag cloud
- Quick stats (total inventories, items, users)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# / .NET 8 |
| Frontend | Blazor Server |
| CSS Framework | Bootstrap 5 |
| ORM | Entity Framework Core |
| Database (local) | SQL Server LocalDB |
| Database (production) | PostgreSQL (Render) |
| Authentication | ASP.NET Core Identity + Google + GitHub OAuth |
| Image Storage | Cloudinary |
| Markdown | Markdig |
| Drag and Drop | SortableJS |
| Deployment | Render (Docker) |

---

## Getting Started (Local)

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB
- Visual Studio 2022 or Rider

### Setup

1. Clone the repository:
```bash
git clone https://github.com/RashfiTabassum/inventory-management-dotnet
cd inventory-management-dotnet
```

2. Create `appsettings.Secrets.json` in `InventoryApp.Web/`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InventoryAppDb;Trusted_Connection=True;"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "GitHub": {
      "ClientId": "YOUR_GITHUB_CLIENT_ID",
      "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
    }
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUD_NAME",
    "ApiKey": "YOUR_API_KEY",
    "ApiSecret": "YOUR_API_SECRET"
  }
}
```

3. Run the application:
```bash
dotnet run --project InventoryApp.Web
```

The database will be created automatically on first run. The admin account (`rashfi2004@gmail.com`) is seeded automatically.

---

## Project Structure

```
InventoryApp/
├── InventoryApp.Core/          # Shared interfaces
├── InventoryApp.Data/          # Models, DbContext, Services, Migrations
│   ├── Models/                 # Domain models
│   ├── Services/               # Business logic services
│   ├── CustomId/               # Custom ID engine
│   └── Seeding/                # Role and admin seeders
└── InventoryApp.Web/           # Blazor Server app
    ├── Components/
    │   ├── Pages/              # All Blazor pages and tab components
    │   └── Layout/             # NavMenu, MainLayout, SearchBox, etc.
    ├── Services/               # LocalizationService, CloudinaryService
    └── Resources/              # Strings.en.json, Strings.ru.json
```

---

## Deployment

The app is deployed on **Render** using Docker.

- PostgreSQL database hosted on Render
- Environment variables set via Render dashboard
- SSL handled at the proxy level
- Schema created automatically via `EnsureCreated` on startup

