using Microsoft.EntityFrameworkCore;
using PressingG3.core.Data;
using PressingG3.core.Entities;
using PressingG3.core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PressingG3.core.Repositories
{
    // Il implémente notre interface IRepository
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly PressingDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(PressingDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync(); // On valide immédiatement
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                // On ne supprime pas vraiment, on archive (Soft Delete)
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.Now;
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}