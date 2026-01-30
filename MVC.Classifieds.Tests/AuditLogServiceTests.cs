using Microsoft.EntityFrameworkCore;
using MVC___MSSQL_Classifieds_Portal.Models;
using MVC___MSSQL_Classifieds_Portal.Services;
using System.Text.Json;

namespace MVC.Classifieds.Tests;

public class AuditLogServiceTests
{
    private readonly ClassifieldsContext _context;
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClassifieldsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClassifieldsContext(options);
        _service = new AuditLogService(_context);
    }

    [Fact]
    public async Task LogAsync_CreatesAuditLogEntry()
    {
        // Arrange
        var changes = new { Title = "Test Listing", Price = 100.00m };

        // Act
        await _service.LogAsync("CREATE", "Listing", 1, 1, changes);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal("CREATE", auditLog.Action);
        Assert.Equal("Listing", auditLog.EntityName);
        Assert.Equal(1, auditLog.EntityId);
        Assert.Equal(1, auditLog.UserId);
        Assert.NotNull(auditLog.Changes);
        Assert.Contains("Test Listing", auditLog.Changes);
    }

    [Fact]
    public async Task LogAsync_WithNullChanges_SavesEntryWithoutChanges()
    {
        // Act
        await _service.LogAsync("DELETE", "Listing", 2, 1, null);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal("DELETE", auditLog.Action);
        Assert.Null(auditLog.Changes);
    }

    [Fact]
    public async Task LogAsync_SetsTimestampToUtcNow()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow.AddSeconds(-1);

        // Act
        await _service.LogAsync("UPDATE", "Listing", 3, 1, new { Title = "Updated" });
        var afterTime = DateTime.UtcNow.AddSeconds(1);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.True(auditLog.Timestamp >= beforeTime);
        Assert.True(auditLog.Timestamp <= afterTime);
    }

    [Fact]
    public async Task LogAsync_SerializesComplexObjects()
    {
        // Arrange
        var complexChanges = new
        {
            Old = new { Title = "Old Title", Price = 50.00m },
            New = new { Title = "New Title", Price = 100.00m }
        };

        // Act
        await _service.LogAsync("UPDATE", "Listing", 4, 1, complexChanges);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.NotNull(auditLog.Changes);
        
        // Verify JSON can be deserialized
        var deserializedChanges = JsonSerializer.Deserialize<JsonElement>(auditLog.Changes);
        Assert.True(deserializedChanges.TryGetProperty("Old", out var oldValue));
        Assert.True(deserializedChanges.TryGetProperty("New", out var newValue));
    }

    [Fact]
    public async Task LogAsync_WithNullEntityId_SavesCorrectly()
    {
        // Act
        await _service.LogAsync("LOGIN", "User", null, 5, null);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal("LOGIN", auditLog.Action);
        Assert.Null(auditLog.EntityId);
        Assert.Equal(5, auditLog.UserId);
    }

    [Fact]
    public async Task LogAsync_WithNullUserId_SavesCorrectly()
    {
        // Act
        await _service.LogAsync("SYSTEM", "Maintenance", 1, null, new { Type = "Cleanup" });

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        Assert.NotNull(auditLog);
        Assert.Equal("SYSTEM", auditLog.Action);
        Assert.Null(auditLog.UserId);
    }

    [Fact]
    public async Task LogAsync_MultipleEntries_AllSavedCorrectly()
    {
        // Act
        await _service.LogAsync("CREATE", "Listing", 1, 1, new { Title = "First" });
        await _service.LogAsync("UPDATE", "Listing", 1, 1, new { Title = "Second" });
        await _service.LogAsync("DELETE", "Listing", 1, 1, new { Title = "Third" });

        // Assert
        var auditLogs = await _context.AuditLogs.ToListAsync();
        Assert.Equal(3, auditLogs.Count);
        Assert.Contains(auditLogs, log => log.Action == "CREATE");
        Assert.Contains(auditLogs, log => log.Action == "UPDATE");
        Assert.Contains(auditLogs, log => log.Action == "DELETE");
    }
}
