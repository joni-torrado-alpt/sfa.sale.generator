using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace sfa.sale.generator.core;
/// <summary>
/// Context for the contacts database.
/// </summary>
public class SfaDbContext : DbContextTransaction
{
    public const string SCHEMA_NAME = "sfa";
    /// <summary>
    /// Magic string.
    /// </summary>
    public static readonly string RowVersion = nameof(RowVersion);

    /// <summary>
    /// Inject options.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions{SfaDBContext}"/>
    /// for the context
    /// </param>
    public SfaDbContext(DbContextOptions<SfaDbContext> options) : base(options)
    {
        Debug.WriteLine($"{ContextId} context created.");
    }

    /// <summary>
    /// List of <see cref="SfaContext"/>.
    /// </summary>
    public DbSet<SfaContext>? SfaContext { get; set; }
    public DbSet<SfaContextClientAddress>? SfaContextClientAddress { get; set; }
    public DbSet<SfaContextOffer>? SfaContextOffer { get; set; }
    public DbSet<SfaContextMasterUser>? SfaContextMasterUser { get; set; }
    public DbSet<SfaSale>? SfaSale { get; set; }
    public DbSet<SfaLog>? SfaLog { get; set; }

    /// <summary>
    /// Define the model.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA_NAME);
        // this property isn't on the C# class
        // so we set it up as a "shadow" property and use it for concurrency
        modelBuilder.Entity<SfaContext>()
            .Property<byte[]>(RowVersion)
            .IsRowVersion();

        OnModelCreating_EnumToTable(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void OnModelCreating_EnumToTable(ModelBuilder modelBuilder)
    {
        modelBuilder.SeedEnumTable<SfaLogTypeEnum, SfaLogType>();
    }

    /// <summary>
    /// Dispose pattern.
    /// </summary>
    public override void Dispose()
    {
        Debug.WriteLine($"{ContextId} context disposed.");
        base.Dispose();
    }

    /// <summary>
    /// Dispose pattern.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/></returns>
    public override ValueTask DisposeAsync()
    {
        Debug.WriteLine($"{ContextId} context disposed async.");
        return base.DisposeAsync();
    }
}
