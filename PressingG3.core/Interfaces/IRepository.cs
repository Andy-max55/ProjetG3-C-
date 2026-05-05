using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
// C'est cette ligne qui corrige l'erreur rouge (CS0234 et CS0246) :
using PressingG3.core.Entities;

namespace PressingG3.core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
    }
}