using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using Microsoft.EntityFrameworkCore.Storage;

namespace HR.ProjectManagement.Repositories;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ApplicationDBContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            return;
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            return;

        await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
