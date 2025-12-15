using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

// Tüm entity türleri için ortak CRUD ve LINQ tabanlı veri erişimi sağlayan generic repository.
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GymAppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    // Constructor - DbContext'i alır ve entity set'ini hazırlar.
    public Repository(GymAppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ID'ye göre entity getirir (PK ile arama).
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    // Tablodaki tüm kayıtları döndürür.
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    // Verilen LINQ ifadesine göre filtrelenmiş kayıtları döndürür.
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    // Verilen koşula göre ilk kaydı (veya null) döndürür.
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    // Yeni bir kayıt ekler ve değişiklikleri kaydeder.
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Var olan bir kaydı günceller.
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Verilen kaydı siler.
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    // Verilen koşula göre en az bir kayıt olup olmadığını kontrol eder.
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    // Toplam kayıt sayısını veya koşula uyan kayıt sayısını döndürür.
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }
}

