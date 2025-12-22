using HR.ProjectManagement.Contracts.Persistence;
using HR.ProjectManagement.DataContext;
using HR.ProjectManagement.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace HR.ProjectManagement.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{

    protected readonly ApplicationDBContext _context;

    public GenericRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(T entity)
    {
        await _context.AddAsync(entity);
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Remove(entity);
    }

    public async Task<IReadOnlyList<T>> GetAsync()
        => await _context.Set<T>()
            .AsNoTracking()
            .ToListAsync();
    

    public async Task<T?> GetByIdAsync(int id)
        => await _context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);
    

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

}
