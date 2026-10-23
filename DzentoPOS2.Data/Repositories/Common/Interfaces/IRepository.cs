using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using DzentoPOS2.Core.Common;
using System.Linq.Expressions;

namespace DzentoPOS2.Data.Repositories.Common.Interfaces
{
    public interface IRepository<TEntity>
     where TEntity : AuditableEntity
    {
        Task<TEntity?> GetByIdAsync(Guid id);

        Task<IReadOnlyList<TEntity>> GetAllAsync();

        Task<IReadOnlyList<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate);

        Task<bool> ExistsAsync(
            Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);
    }
}