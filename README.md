# 🏪 MVC + MSSQL Classifieds Portal

A full-featured classifieds portal built with **ASP.NET Core 8.0 MVC**, **Entity Framework Core**, and **MSSQL LocalDB**. This project demonstrates modern web development practices including authentication, CRUD operations, filtering, pagination, and privacy-respecting user management.

## 🌟 Features

### Core Functionality
- **Listing Management**: Full CRUD operations for classifieds listings
- **Category System**: Organize listings by categories (Electronics, Furniture, Vehicles, etc.)
- **User Authentication**: Secure registration and login with BCrypt password hashing
- **User Profiles**: Personal profile pages showing user info and their listings
- **Search & Filter**: Advanced filtering by title, category, and price range
- **Pagination**: Efficient data display with configurable page sizes
- **Soft Deletes**: Items marked inactive instead of permanent deletion

### Security & Privacy
- **Cookie-based Authentication**: Secure session management
- **Claims-based Authorization**: Role-based access control
- **Privacy-First Design**: Users can only view their own profile, not others
- **Ownership Verification**: Users can only edit/delete their own listings
- **Protected Routes**: `[Authorize]` attribute on sensitive actions

### UI/UX
- **Responsive Bootstrap 5 Design**: Mobile-friendly interface
- **Gradient Hero Section**: Eye-catching landing page with optimized contrast
- **Dropdown Navigation**: User-friendly menu with profile, create listing, and logout options
- **Real-time Statistics**: Active listings and categories count on homepage
- **Recent Listings**: Display of latest items with user and category info

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: MSSQL LocalDB with Entity Framework Core
- **ORM**: Entity Framework Core with migrations
- **Authentication**: Cookie Authentication with BCrypt.Net-Next
- **Mapping**: AutoMapper for DTO/ViewModel conversions
- **Frontend**: Razor Views, Bootstrap 5, HTML5, CSS3
- **Language**: C# 12 (.NET 8)

## 📦 NuGet Packages

```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
```

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- MSSQL LocalDB (included with Visual Studio)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SpaceNamee/MVC-MSSQL-Classifieds-Portal-ASP.NET-Core.git
   cd "MVC-MSSQL-Classifieds-Portal-ASP.NET-Core/MVC + MSSQL Classifieds Portal"
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update the database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```
   Or with hot reload:
   ```bash
   dotnet watch run
   ```

5. **Access the application**
   - Navigate to: `http://localhost:5150`

## 🗄️ Database Schema

### Tables

#### **Users**
- `Id` (PK, int)
- `Username` (unique, required)
- `Email` (unique, required)
- `PasswordHash` (BCrypt hashed)
- `CreatedAt`, `LastLoginAt`
- `IsActive` (soft delete flag)

#### **Categories**
- `Id` (PK, int)
- `Name` (required)
- `Description`
- `CreatedAt`

#### **Listings**
- `Id` (PK, int)
- `Title` (required, max 100 chars)
- `Description`
- `Price` (decimal 18,4)
- `CategoryId` (FK → Categories)
- `UserId` (FK → Users)
- `ImageUrl`
- `CreatedAt`, `LastUpdatedAt`
- `IsActive` (soft delete flag)

#### **AuditLog**
- `Id` (PK, int)
- `Action` (required, max 50 chars)
- `EntityName`, `EntityId`
- `UserId` (FK → Users)
- `Changes` (JSON)
- `Timestamp`

### Relationships
- **User → Listings**: One-to-Many
- **Category → Listings**: One-to-Many
- **User → AuditLog**: One-to-Many

### Indexes
- `IX_Listings_CategoryId`
- `IX_Listings_UserId`
- `IX_Listings_CreatedAt`
- `IX_Listings_IsActive`
- `IX_AuditLog_UserId`
- `IX_AuditLog_Timestamp`

## 🔑 Seed Data

The application comes with pre-seeded data:

### Users
| Username | Password | Email |
|----------|----------|-------|
| alice | Alice123! | alice@example.com |
| bob | Bob123! | bob@example.com |
| aaaa | Aaaa123! | lucachatgpt21@gmail.com |

### Categories
- Electronics
- Furniture
- Vehicles
- Real Estate
- Jobs
- Services

### Sample Listings
- iPhone 14 ($799.99)
- MacBook Pro 14" ($1,899.50)
- AirPods Pro ($199.99)
- Dining Table Set ($450.00)

## 📁 Project Structure

```
MVC + MSSQL Classifieds Portal/
│
├── Controllers/
│   ├── AccountController.cs      # Authentication (Login, Register, Logout)
│   ├── CategoriesController.cs   # Category CRUD
│   ├── HomeController.cs         # Landing page with stats
│   ├── ListingsController.cs    # Listing CRUD + filtering
│   └── UsersController.cs       # User profile management
│
├── Models/
│   ├── AuditLog.cs              # Audit logging entity
│   ├── Category.cs              # Category entity
│   ├── ClassifieldsContext.cs   # EF Core DbContext
│   ├── ErrorViewModel.cs        # Error handling
│   ├── Listing.cs               # Listing entity
│   ├── User.cs                  # User entity
│   └── ViewModels/
│       ├── ListingFilterViewModel.cs  # Filtering/pagination model
│       └── ListingViewModel.cs        # Listing display model
│
├── Views/
│   ├── Account/                 # Login, Register, AccessDenied
│   ├── Categories/              # Category views
│   ├── Home/                    # Index page
│   ├── Listings/                # CRUD + Index with filters
│   ├── Shared/                  # Layout, navigation
│   └── Users/                   # Profile, CRUD (admin)
│
├── Mappings/
│   └── MappingProfile.cs        # AutoMapper configuration
│
├── Migrations/                  # EF Core migrations
├── wwwroot/                     # Static files (CSS, JS, images)
├── appsettings.json             # Configuration
└── Program.cs                   # Application entry point
```

## 🎨 Key Features Breakdown

### 1. Authentication System
```csharp
// Registration with BCrypt hashing
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Login with password verification
bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

// Cookie authentication
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(claimsIdentity)
);
```

### 2. Listing Filtering & Pagination
- **Search by title**: Case-insensitive contains search
- **Filter by category**: Dropdown selection
- **Price range**: Min/Max price filters
- **Sorting**: Price (low-to-high, high-to-low), Date (newest, oldest)
- **Pagination**: Configurable page size with page navigation

### 3. Soft Delete Pattern
```csharp
// Global query filter - automatically excludes soft-deleted items
modelBuilder.Entity<Listing>()
    .HasQueryFilter(l => l.IsActive);

// Soft delete instead of hard delete
listing.IsActive = false;
listing.LastUpdatedAt = DateTime.UtcNow;
```

### 4. Query Optimization
```csharp
// AsNoTracking() for read-only queries - improves performance
var listings = await _context.Listings
    .AsNoTracking()
    .Include(l => l.Category)
    .Include(l => l.User)
    .ToListAsync();
```

### 5. Privacy Protection
```csharp
[Authorize]
public async Task<IActionResult> Profile()
{
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var user = await _context.Users
        .Include(u => u.Listings.Where(l => l.IsActive))
        .ThenInclude(l => l.Category)
        .FirstOrDefaultAsync(u => u.Id == userId);
    
    return View(user);
}
```

## 🔒 Security Features

1. **Password Security**: BCrypt hashing with salt (cost factor 11)
2. **SQL Injection Protection**: Parameterized queries via EF Core
3. **XSS Protection**: Razor automatic HTML encoding
4. **CSRF Protection**: Anti-forgery tokens on forms
5. **Authorization**: Claims-based access control
6. **Ownership Verification**: Users can only modify their own data

## 🎯 User Flows

### New User Registration
1. Click "Register" → Fill form (username, email, password)
2. Password hashed with BCrypt → User created in DB
3. Auto-login → Redirect to homepage

### Creating a Listing
1. Login required → Navigate to "Create New Listing"
2. Fill form (title, description, price, category, image URL)
3. Ownership automatically assigned to current user
4. Listing visible on homepage and listings index

### Viewing Profile
1. Click username dropdown → "My Profile"
2. See personal info + all your active listings
3. Edit/Delete buttons available only for your listings

## 📊 Performance Optimizations

- **AsNoTracking()**: Used on read-only queries to reduce memory overhead
- **Eager Loading**: `.Include()` to prevent N+1 query problems
- **Pagination**: Only load required page data, not entire dataset
- **Indexes**: Composite indexes on frequently queried columns
- **Soft Deletes**: Faster than hard deletes (no cascade operations)

## 🧪 Testing Credentials

Use these accounts to test the application:

```
Username: alice
Password: Alice123!

Username: bob
Password: Bob123!

Username: aaaa
Password: Aaaa123!
```

## 🐛 Known Issues

- **CS8618 Warnings**: Nullable reference type warnings (cosmetic, non-critical)
  - Can be fixed by adding `required` keyword or nullable annotations
- **Database Migration Error**: Initial migration may fail if tables already exist
  - Solution: Drop database and re-run `dotnet ef database update`

## 🚧 Future Enhancements

- [ ] Edit Profile / Change Password functionality
- [ ] User avatar upload
- [ ] Image upload for listings (currently URL only)
- [ ] Messaging system between buyers/sellers
- [ ] Favorites/Watchlist functionality
- [ ] Email verification for registration
- [ ] Password reset via email
- [ ] User reputation/rating system
- [ ] Advanced search with multiple filters
- [ ] Export listings to PDF/CSV

## 📝 Development Notes

### Running Migrations
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database to latest migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Drop database
dotnet ef database drop
```

### Hot Reload
The application supports hot reload for rapid development:
```bash
dotnet watch run
```
Changes to Razor views are reflected immediately without restart.

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 👥 Authors

- **SpaceNamee** - [GitHub Profile](https://github.com/SpaceNamee)

## 🙏 Acknowledgments

- ASP.NET Core documentation
- Entity Framework Core documentation
- Bootstrap 5 framework
- BCrypt.Net-Next library
- AutoMapper library

## 📞 Support

For issues, questions, or suggestions:
- Open an [Issue](https://github.com/SpaceNamee/MVC-MSSQL-Classifieds-Portal-ASP.NET-Core/issues)
- Contact: lucachatgpt21@gmail.com

---

**Built with ❤️ using ASP.NET Core 8.0**
