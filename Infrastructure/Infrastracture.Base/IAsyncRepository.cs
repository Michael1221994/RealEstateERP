using Infrastracture.Base.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base
{
    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }
    public interface IRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Task<IEnumerable<TEntity>> ExecuteQuery<TEntity>(string query, params object[] parameters) where TEntity : class;
        Task AddAsync<TEntity>(TEntity entity) where TEntity : class;
        Task AttachAsync<TEntity>(TEntity entity) where TEntity : class;
        Task<int> CountAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;
        Task<int> CountAsync<TEntity>() where TEntity : class;
        Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;
        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class;
        Task DeleteAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task<IEnumerable<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;
        Task<IEnumerable<TEntity>> FindAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task<TEntity> FindOneAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;
        Task<TEntity> FindOneAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task<TEntity> FirstAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task<TEntity> FirstAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(ISpecification<TEntity> specification, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class;
        Task<TEntity> GetByKeyAsync<TEntity>(object keyValue) where TEntity : class;
        Task<IQueryable<TEntity>> GetQueryAsync<TEntity>(ISpecification<TEntity> criteria,bool forUpdate=false) where TEntity : class;
        Task<IQueryable<TEntity>> GetQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, bool forUpdate = false) where TEntity : class;
        Task<IQueryable<TEntity>> GetQueryAsync<TEntity>( bool forUpdate = false) where TEntity : class;
        Task<TEntity> SingleAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class;
        Task<TEntity> SingleAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class;
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class;
        public DateTime SetKindUtc(DateTime dateTime);
    }
}
