using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using ToursAndTravelsManagement.Common;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly PropertyInfo _primaryKeyProperty;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _primaryKeyProperty = GetPrimaryKeyProperty();
    }
    private PropertyInfo GetPrimaryKeyProperty()
    {
        var entityType = typeof(T);
        var keyProperties = entityType.GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), true).Any())
            .ToList();

        if (keyProperties.Count != 1)
        {
            throw new InvalidOperationException($"Entity type {entityType.Name} has no or multiple Key attributes.");
        }

        return keyProperties.Single();
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter = null, string includeProperties = null)
    {
        IQueryable<T> query = _dbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }
        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }
        return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id, string includeProperties = null)
    {
        IQueryable<T> query = _dbSet.Where(e => EF.Property<int>(e, _primaryKeyProperty.Name) == id);

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
    public async Task<PaginatedList<T>> GetPaginatedAsync(int pageNumber, int pageSize, string includeProperties = null)
    {
        IQueryable<T> query = _dbSet;

        if (!string.IsNullOrEmpty(includeProperties))
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
        }

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
    }
}