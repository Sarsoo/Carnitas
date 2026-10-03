using Carnitas.Model;
using Carnitas.Model.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Carnitas.Test;

/// <summary>
/// Guards against the duplicate-relationship regression that produced a shadow
/// foreign key property <c>OperationRunId1</c> on <see cref="OperationRunLogEntry"/>.
/// Building the model does not require a database connection.
/// </summary>
public class OperationRunLogEntryModelTests
{
    private static IEntityType GetLogEntryEntityType()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost; Database=carnitas; Username=andy")
            .Options;

        using var db = new ApplicationDbContext(options);
        return db.Model.FindEntityType(typeof(OperationRunLogEntry))!;
    }

    [Fact]
    public void LogEntry_has_exactly_one_foreign_key_to_OperationRun()
    {
        var entity = GetLogEntryEntityType();

        var fks = entity.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(OperationRun))
            .ToList();

        Assert.Single(fks);
    }

    [Fact]
    public void LogEntry_has_no_shadow_OperationRunId1_property()
    {
        var entity = GetLogEntryEntityType();

        Assert.DoesNotContain(entity.GetProperties(), p => p.Name == "OperationRunId1");
    }

    [Fact]
    public void LogEntry_foreign_key_uses_OperationRunId_and_LogEntries_inverse()
    {
        var entity = GetLogEntryEntityType();

        var fk = entity.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(OperationRun));

        Assert.Equal(nameof(OperationRunLogEntry.OperationRunId), fk.Properties.Single().Name);
        Assert.Equal(nameof(OperationRun.LogEntries), fk.PrincipalToDependent?.Name);
    }
}
