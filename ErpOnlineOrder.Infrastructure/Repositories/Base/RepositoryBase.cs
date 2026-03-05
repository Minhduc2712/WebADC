using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace ErpOnlineOrder.Infrastructure.Repositories.Base
{
    public class RepositoryBase<T> where T : class
    {
        protected readonly ErpOnlineOrderDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(ErpOnlineOrderDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsQueryable();
        }

        public IQueryable<T> GetAllActive()
        {
            var query = _dbSet.AsQueryable();
            var filter = CreateIsNotDeletedExpression();
            if (filter != null)
                query = query.Where(filter);
            return query;
        }

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>Soft delete: set Is_deleted = true.</summary>
        public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity != null)
            {
                SetIsDeleted(entity, true);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>Tạo expression e => !e.Is_deleted cho entity có property Is_deleted.</summary>
        private static Expression<Func<T, bool>>? CreateIsNotDeletedExpression()
        {
            var prop = typeof(T).GetProperty("Is_deleted", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.PropertyType != typeof(bool))
                return null;

            var param = Expression.Parameter(typeof(T), "e");
            var propAccess = Expression.Property(param, prop);
            var notDeleted = Expression.Not(propAccess);
            return Expression.Lambda<Func<T, bool>>(notDeleted, param);
        }

        private static void SetIsDeleted(T entity, bool value)
        {
            var prop = typeof(T).GetProperty("Is_deleted", BindingFlags.Public | BindingFlags.Instance);
            prop?.SetValue(entity, value);
        }
    }
}
