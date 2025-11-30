using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GymApp.Data;
using GymApp.Entities;

namespace GymApp.Repositories;

/// <summary>
/// Generic Repository Pattern - Tüm entity'ler için temel CRUD işlemlerini sağlar
/// CRUD işlemleri: Create (AddAsync), Read (GetByIdAsync, GetAllAsync, FindAsync), Update (UpdateAsync), Delete (DeleteAsync)
/// LINQ sorguları: Expression<Func<T, bool>> ile dinamik filtreleme desteği
/// </summary>
/// <typeparam name="T">Entity tipi (class olmalı)</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GymAppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructor - DbContext'i alır ve entity set'ini hazırlar
    /// </summary>
    public Repository(GymAppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// GetByIdAsync - ID'ye göre entity getirir
    /// Read işlemi: Primary key ile entity çekme
    /// </summary>
    /// <param name="id">Entity ID'si</param>
    /// <returns>Entity bulunursa T nesnesi, bulunamazsa null</returns>
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <summary>
    /// GetAllAsync - Tüm entity'leri getirir
    /// Read işlemi: Tüm kayıtları listeleme
    /// </summary>
    /// <returns>Entity listesi</returns>
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <summary>
    /// FindAsync - LINQ sorgusu ile entity'leri filtreler
    /// Read işlemi: Koşula göre filtreleme
    /// LINQ sorgusu: Expression<Func<T, bool>> ile dinamik filtreleme
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (LINQ expression)</param>
    /// <returns>Filtrelenmiş entity listesi</returns>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <summary>
    /// FirstOrDefaultAsync - LINQ sorgusu ile ilk entity'yi getirir
    /// Read işlemi: Koşula göre ilk kaydı bulma
    /// LINQ sorgusu: Expression<Func<T, bool>> ile dinamik filtreleme
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (LINQ expression)</param>
    /// <returns>İlk entity veya null</returns>
    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// AddAsync - Yeni entity ekler
    /// Create işlemi: Veritabanına yeni kayıt ekleme
    /// </summary>
    /// <param name="entity">Eklenecek entity</param>
    /// <returns>Eklenen entity (ID ile birlikte)</returns>
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// UpdateAsync - Mevcut entity'yi günceller
    /// Update işlemi: Veritabanında mevcut kaydı güncelleme
    /// </summary>
    /// <param name="entity">Güncellenecek entity</param>
    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// DeleteAsync - Entity'yi siler
    /// Delete işlemi: Veritabanından kayıt silme
    /// </summary>
    /// <param name="entity">Silinecek entity</param>
    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ExistsAsync - Koşula uyan entity'nin var olup olmadığını kontrol eder
    /// Validation: Varlık kontrolü için kullanılır
    /// LINQ sorgusu: Expression<Func<T, bool>> ile dinamik kontrol
    /// </summary>
    /// <param name="predicate">Kontrol koşulu (LINQ expression)</param>
    /// <returns>Koşula uyan entity varsa true, yoksa false</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// CountAsync - Entity sayısını getirir
    /// Read işlemi: Toplam kayıt sayısı veya koşula uyan kayıt sayısı
    /// LINQ sorgusu: Opsiyonel predicate ile filtreleme
    /// </summary>
    /// <param name="predicate">Filtreleme koşulu (opsiyonel - null ise tüm kayıtlar sayılır)</param>
    /// <returns>Kayıt sayısı</returns>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
        
        return await _dbSet.CountAsync(predicate);
    }
}

