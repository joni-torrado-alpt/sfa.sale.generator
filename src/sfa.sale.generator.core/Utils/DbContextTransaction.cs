using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace sfa.sale.generator.core;

public abstract class DbContextTransaction : DbContext
{
    public IUserSession _userSession;
    private IDbContextTransaction _currentTransaction;
    private string _changedBy;

    public DbContextTransaction(DbContextOptions options) : base(options)
    {
    }

    public DbContextTransaction(IUserSession userSession) : base()
        => _userSession = userSession;

    public DbContextTransaction(DbContextOptions options, IUserSession userSession) : base(options)
        => _userSession = userSession;

    public bool HasActiveTransaction
        => _currentTransaction != null;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveEntitiesAudit();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public IDbContextTransaction GetCurrentTransaction()
        => _currentTransaction;

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        if (_currentTransaction != null) return null;
        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        if (transaction is null) throw new ArgumentNullException(nameof(transaction));
        if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public Task RollbackTransactionAsync()
    {
        try
        {
            ChangeTracker.Clear();  //Clear all the entities tracks for new transactions. Check if there is a way to do it automatically.
            _currentTransaction?.RollbackAsync();
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
        return Task.CompletedTask;
    }

    public virtual void SetAuditWhenNoSession(string changedBy)
        => _changedBy = changedBy;

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     => optionsBuilder.UseExceptionProcessor();

    private void SaveEntitiesAudit()
    {
        var entities = ChangeTracker.Entries().Where(e => new[] { EntityState.Added, EntityState.Modified, EntityState.Deleted }.Contains(e.State));
        foreach (var item in entities)
        {
            switch (item.Entity)
            {
                case BaseEntity be:
                    SetBaseEntityAudit(be, item);
                    break;

                default:
                    break;
            }
        }
    }

    private void SetBaseEntityAudit(BaseEntity baseEntity, EntityEntry item)
    {
        var currentDateTime = DateTime.Now;
        var currentUser = _userSession?.UserName ?? "Unauthenticated";
        bool hasChangedByAndNoLogin = string.IsNullOrEmpty(_userSession?.UserName) && !string.IsNullOrEmpty(_changedBy);
        var currentChangedBy = hasChangedByAndNoLogin ? _changedBy : currentUser;

        if (item.State == EntityState.Added)
        {
            baseEntity.CreatedOn = currentDateTime;
            baseEntity.CreatedBy = currentChangedBy;
        }
        else
        {
            item.Property(nameof(baseEntity.CreatedOn)).IsModified = false;
            item.Property(nameof(baseEntity.CreatedBy)).IsModified = false;
            baseEntity.ModifiedOn = currentDateTime;
            baseEntity.ModifiedBy = currentChangedBy;
        }
    }
}

public interface IUserSession
{
    bool IsAuthenticated { get; }
    string SecurityToken { get; }
    long UserId { get; }
    string UserName { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<KeyValuePair<string, string>> ExposedClaims { get; }
    void SetPrincipal(ClaimsPrincipal claimsPrincipal, string securityToken = null);
}