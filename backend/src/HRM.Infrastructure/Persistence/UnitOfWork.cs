using HRM.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRM.Infrastructure.Persistence;

/// <inheritdoc cref="IUnitOfWork"/>
public class UnitOfWork : IUnitOfWork
{
    private readonly HrmDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(HrmDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        // Database.BeginTransactionAsync() is a relational-only extension —
        // the EF Core InMemory provider used by HRM.Api.Tests isn't
        // relational and throws if called. A single SaveChangesAsync call
        // is already atomic on any provider, so skipping the explicit
        // transaction there is a safe fallback, not a correctness gap for
        // that provider; the real SQL Server provider always gets a real
        // transaction.
        if (_context.Database.IsRelational())
        {
            _transaction = await _context.Database.BeginTransactionAsync(ct);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
