using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Inventory.InfraStructure.Repositories;

public class RepositoreBase<T>(DataContext context) : IRepositoryBase<T>
    where T : class
{
    private readonly DataContext _context = context;

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();

    public virtual async Task<T?> GetByIdAsync(Guid id)
        => await Task.FromResult(_context.Set<T>().Find(id));

    public virtual async Task<T?> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync();
    }
}