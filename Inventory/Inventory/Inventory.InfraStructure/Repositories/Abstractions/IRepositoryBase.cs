namespace Inventory.InfraStructure.Repositories.Abstractions;

public interface IRepositoryBase<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T?>  AddAsync (T entity);
    Task UpdateAsync (T entity);
    Task DeleteAsync (T entity);
}