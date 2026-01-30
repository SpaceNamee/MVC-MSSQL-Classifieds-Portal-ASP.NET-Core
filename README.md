# MVC + MSSQL Classifieds Portal

A full-featured classifieds portal built with ASP.NET Core 8.0 MVC, Entity Framework Core, and MSSQL LocalDB. This project demonstrates modern web development practices including authentication, CRUD operations, filtering, pagination, and privacy-respecting user management.

## Features

### Core Functionality
- **Listing Management**: Complete CRUD operations for classifieds listings with soft delete support
- **Category System**: Organized categorization (Electronics, Furniture, Vehicles, Real Estate, Jobs, Services)
- **User Authentication**: Secure registration and login with BCrypt password hashing (cost factor 11)
- **User Profiles**: Personal profile pages displaying user information and their active listings
- **Advanced Search & Filter**: Filter by title, category, and price range with multiple sorting options
- **Pagination**: Efficient data display with configurable page sizes (default 10 items per page)
- **Soft Deletes**: Items marked inactive instead of permanent deletion for data integrity

### Security & Privacy
- **Cookie-based Authentication**: Secure session management with ASP.NET Core Identity
- **Claims-based Authorization**: Fine-grained access control using ClaimTypes
- **Privacy-First Design**: Users can only view their own profile, preventing data leakage
- **Ownership Verification**: Users can only edit or delete their own listings
- **Protected Routes**: Authorization attributes on sensitive controller actions
- **SQL Injection Protection**: Parameterized queries via Entity Framework Core
- **XSS Protection**: Automatic HTML encoding in Razor views
- **CSRF Protection**: Anti-forgery tokens on all state-changing operations

### UI/UX Design
- **Responsive Bootstrap 5**: Mobile-first design with breakpoint optimization
- **Gradient Hero Section**: Modern landing page with accessibility-compliant contrast ratios
- **Dropdown Navigation**: Intuitive menu system with profile, create listing, and logout options
- **Real-time Statistics**: Dashboard displaying active listings and categories count
- **Recent Listings Grid**: Latest items with thumbnail, price, category, and seller information

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC with C# 12
- **Database**: MSSQL LocalDB with Entity Framework Core 8.0.10
- **ORM**: Entity Framework Core with Code-First migrations
- **Authentication**: Cookie Authentication with BCrypt.Net-Next for password hashing
- **Object Mapping**: AutoMapper 12.0.1 for DTO/ViewModel transformations
- **Frontend**: Razor Views, Bootstrap 5, HTML5, CSS3
- **API Documentation**: Swashbuckle.AspNetCore (Swagger)

## Dependencies

### Main Application
```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.10" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
```

### Test Project (MVC.Classifieds.Tests)
```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.13.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.2" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.2" />
```

## Getting Started

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
   This applies 3 migrations:
   - Initial schema creation
   - Relationship constraints and indexes (8 indexes)
   - AuditLog table and timestamp index

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

## Database Schema

### Tables

**Users**
- `Id` (PK, int, identity)
- `Username` (nvarchar(50), unique, required, indexed)
- `Email` (nvarchar(100), unique, required)
- `PasswordHash` (nvarchar(255), BCrypt hashed)
- `CreatedAt` (datetime2, default: GETUTCDATE())
- `LastLoginAt` (datetime2, nullable)
- `IsActive` (bit, soft delete flag)

**Categories**
- `Id` (PK, int, identity)
- `Name` (nvarchar(50), required)
- `Description` (nvarchar(max), nullable)
- `CreatedAt` (datetime2)

**Listings**
- `Id` (PK, int, identity)
- `Title` (nvarchar(100), required, indexed)
- `Description` (nvarchar(max), nullable)
- `Price` (decimal(18,4), required)
- `CategoryId` (FK, required)
- `UserId` (FK, required)
- `ImageUrl` (nvarchar(500), nullable)
- `CreatedAt` (datetime2, indexed)
- `LastUpdatedAt` (datetime2)
- `IsActive` (bit, indexed for soft deletes)

**AuditLog**
- `Id` (PK, int, identity)
- `Action` (nvarchar(50), required) - CREATE, UPDATE, DELETE operations
- `EntityName` (nvarchar(100)) - Target entity type (Listing, Category, User)
- `EntityId` (int, nullable) - ID of modified entity
- `UserId` (FK, nullable) - User who performed the action
- `Changes` (nvarchar(max), JSON format) - Serialized before/after state
- `Timestamp` (datetime2, indexed) - UTC timestamp of action

### Audit Logging System

**AuditLogService** provides automatic change tracking for all CRUD operations:

```csharp
// Injected via dependency injection
public AuditLogService(ClassifieldsContext context)

// Log creation
await _auditLogService.LogAsync("CREATE", "Listing", listing.Id, userId, new { listing.Title, listing.Price });

// Log updates with changes
await _auditLogService.LogAsync("UPDATE", "Listing", listing.Id, userId, new { OldPrice = oldPrice, NewPrice = listing.Price });

// Log deletions
await _auditLogService.LogAsync("DELETE", "Listing", listingId, userId, new { Title = listing.Title });
```

**Logged Operations:**
- Listing creation, updates, soft deletes
- Category management changes
- User profile modifications
- Failed authorization attempts (future enhancement)

**Query Examples:**
```csharp
// Get recent activity for a specific entity
var logs = await _context.AuditLogs
    .Where(a => a.EntityName == "Listing" && a.EntityId == listingId)
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();

// Get all activity by a user
var userActivity = await _context.AuditLogs
    .Where(a => a.UserId == userId)
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();

// Get recent system-wide activity
var recentActivity = await _context.AuditLogs
    .OrderByDescending(a => a.Timestamp)
    .Take(100)
    .ToListAsync();
```

**Performance Considerations:**
- Indexed on `Timestamp` (descending) for efficient chronological queries
- JSON serialization keeps storage compact
- Async operations prevent blocking main request threads

### Relationships
- `User` → `Listings`: One-to-Many (CASCADE on delete)
- `Category` → `Listings`: One-to-Many (RESTRICT on delete)
- `User` → `AuditLog`: One-to-Many (SET NULL on delete)

### Composite Indexes
- `IX_Listings_CategoryId_IsActive` (filtering performance)
- `IX_Listings_UserId_IsActive` (user listings queries)
- `IX_Listings_CreatedAt_DESC` (sorting by date)
- `IX_AuditLog_Timestamp_DESC` (audit trail queries)

## Seed Data

Pre-configured test accounts:

| Username | Password | Email |
|----------|----------|-------|
| alice | Alice123! | alice@example.com |
| bob | Bob123! | bob@example.com |

**Categories**
- Electronics
- Furniture
- Vehicles
- Real Estate
- Jobs
- Services

**Sample Listings**
- iPhone 14 - $799.99 (Electronics, alice)
- MacBook Pro 14" - $1,899.50 (Electronics, alice)
- AirPods Pro - $199.99 (Electronics, alice)
- Dining Table Set - $450.00 (Furniture, alice)

## Project Structure

```
MVC-MSSQL-Classifieds-Portal-ASP.NET-Core/
│
├── MVC + MSSQL Classifieds Portal/       # Main application
│   ├── Controllers/
│   │   ├── AccountController.cs           # Authentication (Login, Register, Logout)
│   │   ├── CategoriesController.cs        # Category CRUD operations
│   │   ├── HomeController.cs              # Landing page with statistics
│   │   ├── ListingsController.cs          # Listing CRUD with audit logging
│   │   └── UsersController.cs             # User profile management
│   │
│   ├── Models/
│   │   ├── AuditLog.cs                   # Audit trail entity
│   │   ├── Category.cs                   # Category entity
│   │   ├── ClassifieldsContext.cs        # EF Core DbContext with indexes
│   │   ├── ErrorViewModel.cs             # Error handling model
│   │   ├── Listing.cs                    # Listing entity with validations
│   │   ├── User.cs                       # User entity
│   │   └── ViewModels/
│   │       ├── ListingFilterViewModel.cs # Filter & pagination
│   │       └── ListingViewModel.cs       # Listing display DTO
│   │
│   ├── Services/
│   │   └── AuditLogService.cs            # Change tracking service
│   │
│   ├── Views/
│   │   ├── Account/                      # Authentication views
│   │   ├── Categories/                   # Category management views
│   │   ├── Home/                         # Dashboard and landing page
│   │   ├── Listings/                     # Listing CRUD and index with filters
│   │   ├── Shared/                       # Layout, navigation, components
│   │   └── Users/                        # User profile and management
│   │
│   ├── Mappings/
│   │   └── MappingProfile.cs             # AutoMapper entity-to-DTO mappings
│   │
│   ├── Migrations/                       # EF Core database migrations (8 indexes)
│   ├── wwwroot/                          # Static assets (CSS, JS, images)
│   ├── appsettings.json                  # Application configuration
│   └── Program.cs                        # Application bootstrap and DI setup
│
└── MVC.Classifieds.Tests/                # Test project (23 tests)
    ├── ListingsControllerUnitTests.cs    # 9 controller tests with mocks
    ├── AuditLogServiceUnitTests.cs       # 7 service layer tests
    ├── ListingsIntegrationTests.cs       # 7 database integration tests
    └── MVC.Classifieds.Tests.csproj      # Test dependencies (xUnit, Moq, InMemory)
```

## Implementation Details

### Authentication System
```csharp
// Registration with BCrypt hashing (cost factor: 11)
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

// Login verification
bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

// Cookie authentication with claims
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Email, user.Email)
};
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme))
);
```

### Listing Filtering & Pagination
**Filter Options:**
- Search by title (case-insensitive partial match)
- Filter by category (dropdown selection)
- Price range filters (min/max decimal values)
- Sort options: Price (ascending/descending), Date (newest/oldest)
- Pagination with configurable page size (default: 10)

### Security Features

**Password Security**
- BCrypt hashing with automatic salt generation
- Configurable cost factor (default: 11, 2048 iterations)
- Rainbow table attack resistant

**SQL Injection Protection**
- Parameterized queries via Entity Framework Core
- No raw SQL string concatenation
- Input validation at model level

**XSS Protection**
- Automatic HTML encoding in Razor views
- Content Security Policy headers
- ValidateAntiForgeryToken on state-changing operations

**CSRF Protection**
- Anti-forgery tokens automatically generated
- Token validation on POST/PUT/DELETE requests
- SameSite cookie attribute set to Strict

## Performance Optimizations

**Query Optimization**
- `AsNoTracking()` on read-only queries (30-40% faster)
- Eager loading with `.Include()` to prevent N+1 queries
- Projection with `Select()` for minimal data transfer
- Filtered queries use composite indexes for optimal performance

**Database Indexing Strategy**

All indexes are applied via Entity Framework Core migrations for consistency.

**Single-Column Indexes:**
- `IX_Listings_CategoryId` - Foreign key lookups for category filtering
- `IX_Listings_UserId` - User's listings queries
- `IX_Listings_Price` - Price range filtering and sorting
- `IX_Users_Username` - Unique constraint + fast username lookups
- `IX_Users_Email` - Unique constraint + email verification

**Composite Indexes:**
- `IX_Listings_CategoryId_IsActive` - Combined category + active status filter (most common query pattern)
- `IX_Listings_UserId_IsActive` - User listings + active status
- `IX_Listings_IsActive_CreatedAt_DESC` - Active listings sorted by date (default homepage query)

**Specialized Indexes:**
- `IX_Listings_Title` - Full-text search enablement (future enhancement)
- `IX_AuditLog_Timestamp_DESC` - Chronological audit trail queries
- `IX_AuditLog_EntityName_EntityId` - Entity-specific audit history

**Index Benefits:**
- **Query Speed**: 10-100x faster on filtered queries with large datasets
- **Join Performance**: Foreign key indexes eliminate table scans
- **Sorting Efficiency**: Descending indexes avoid expensive sort operations
- **Unique Constraints**: Prevents duplicate usernames/emails at database level

**Index Maintenance:**
```sql
-- Check index fragmentation (run periodically)
SELECT 
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent
FROM sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE avg_fragmentation_in_percent > 10;

-- Rebuild fragmented indexes
ALTER INDEX IX_Listings_IsActive_CreatedAt_DESC ON Listings REBUILD;
```

**Pagination**
- Server-side pagination with `Skip()` and `Take()`
- Count query optimization with filtered counts
- Page size limited to prevent resource exhaustion
- Index-optimized sorting (uses IX_Listings_IsActive_CreatedAt_DESC)

## Automated Testing

### Test Suite Coverage
**23 Total Tests** (16 unit tests + 7 integration tests)

**Unit Tests** (ListingsController, AuditLogService)
- 9 controller action tests with mocked dependencies
- 7 audit log service tests covering all logging scenarios
- Mocking framework: Moq 4.20.72
- Assertions: xUnit framework

**Integration Tests** (Database Operations)
- 7 end-to-end database tests with in-memory provider
- Validates relationships, indexes, and service integration
- Tests soft delete, queries, and audit logging

### Running Tests
```bash
# Run all tests
cd MVC.Classifieds.Tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run with code coverage (requires coverlet)
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=opencover
```

### Test Structure
```
MVC.Classifieds.Tests/
├── ListingsControllerUnitTests.cs   # Controller action tests
├── AuditLogServiceUnitTests.cs      # Service layer tests
└── ListingsIntegrationTests.cs      # Database integration tests
```

### Test Accounts
```
alice / Alice123!
bob / Bob123!
```

### Manual Testing Scenarios
1. **Registration**: Create new user → Verify password hashing → Login successful
2. **Authentication**: Login → Verify claims → Access protected routes
3. **CRUD Operations**: Create listing → Edit own listing → Delete own listing
4. **Authorization**: Attempt to edit another user's listing → Verify Forbid() response
5. **Search & Filter**: Apply multiple filters → Verify query results
6. **Pagination**: Navigate through pages → Verify correct offsets
7. **Audit Trail**: Perform actions → Check AuditLog table for records

## Known Issues & Limitations

**CS8618 Nullable Warnings**
- Non-nullable properties without explicit initialization
- Resolution: Add `required` keyword or null-forgiving operator
- Impact: Cosmetic only, runtime behavior unaffected

**Migration Conflicts**
- Initial migration fails if database already exists
- Resolution: `dotnet ef database drop --force` then re-migrate
- Cause: LocalDB instance persists between sessions

**Image Upload**
- Currently accepts only URL strings
- No file upload validation
- No image processing or thumbnail generation

## Development

### Running Migrations
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database to latest migration
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove

# Drop database completely
dotnet ef database drop --force
```

### Hot Reload Development
```bash
# Start with hot reload enabled
dotnet watch run

# Hot reload automatically applies:
# - Razor view changes
# - CSS modifications
# - Static file updates

# Requires restart for:
# - C# code changes
# - Configuration changes
# - Dependency injection modifications
```

## Contributing

Prerequisites:
- Fork the repository
- Create feature branch from master
- Follow existing code conventions
- Write descriptive commit messages

Pull Request Process:
1. Update documentation for new features
2. Ensure all tests pass
3. Submit PR with detailed description

## License

This project is open source and available under the MIT License.

## Author

**Andrea Cazzato**
- GitHub: [@SpaceNamee](https://github.com/SpaceNamee)
- Email: cazzatoandrea@protonmail.com

## Acknowledgments

- ASP.NET Core Team
- Entity Framework Core contributors
- Bootstrap framework
- BCrypt.Net-Next library
- AutoMapper project

## Support

For issues or questions:
- Open an [Issue](https://github.com/SpaceNamee/MVC-MSSQL-Classifieds-Portal-ASP.NET-Core/issues)
- Include error messages and reproduction steps

---

**Week 4 Enhancements (January 2025):**
- ✅ Database Performance: 8 strategic indexes for query optimization
- ✅ Audit Logging: Automatic change tracking with AuditLogService
- ✅ Comprehensive Testing: 23 tests (16 unit + 7 integration) with xUnit and Moq
- ✅ Documentation: Updated README with testing, indexes, and audit log guides

Built with ASP.NET Core 8.0 | Last Updated: January 2025
