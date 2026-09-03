
using Infrastracture.Base.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Infrastracture.Base.EF
{
    public class GenericRepository : IRepository
    {
        private DbContext _context;
        /// <summary>
        /// Initializes a new instance of the <see cref="Repository&lt;TEntity&gt;"/> class.
        /// </summary>
        public GenericRepository()
        {
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="GenericRepository&lt;TEntity&gt;"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public GenericRepository(DbContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            _context = context;
        }

        public IUnitOfWork UnitOfWork
        {
            get
            {
                if (unitOfWork == null)
                {
                    unitOfWork = new UnitOfWork(this._context);
                }
                return unitOfWork;
            }
        }

        public async Task<IEnumerable<TEntity>> ExecuteQuery<TEntity>(string query,params object[] parameters) where TEntity:class
        {
            return DbContext.Database.SqlQueryRaw<TEntity>(query,parameters).AsEnumerable();
              
        }

        public async Task<int> ExecuteScalar(string query, params object[] parameters)
        {
            return await DbContext.Database.ExecuteSqlRawAsync(query, parameters);
        }

        public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            await DbContext.Set<TEntity>().AddAsync(entity);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            DbContext.Set<TEntity>().Remove(entity);
        }

        public async Task AttachAsync<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            DbContext.Set<TEntity>().Attach(entity);
        }

        public async Task<int> CountAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            return await criteria.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>()).CountAsync();
        }

        public async Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return await (await GetQueryAsync<TEntity>()).CountAsync(criteria);
        }

        public async Task<int> CountAsync<TEntity>() where TEntity : class
        {
            return await (await GetQueryAsync<TEntity>()).CountAsync();
        }

        public async Task DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            IEnumerable<TEntity> records = await FindAsync(criteria);

            foreach (TEntity record in records)
            {
                await DeleteAsync(record);
            }
        }

        public async Task DeleteAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            IEnumerable<TEntity> records = await FindAsync(criteria);

            foreach (TEntity record in records)
            {
                await DeleteAsync(record);
            }
        }

        public async Task<IEnumerable<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return (await GetQueryAsync<TEntity>()).Where(criteria);
        }

        public async Task<IEnumerable<TEntity>> FindAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            return criteria.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>()).AsEnumerable();
        }

        public async Task<TEntity> FindOneAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return await (await GetQueryAsync<TEntity>()).Where(criteria).FirstOrDefaultAsync();
        }

        public async Task<TEntity> FindOneAsyncForDelete<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return await (await GetQueryAsync<TEntity>()).Where(criteria).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<TEntity> FindOneAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            return criteria.SatisfyingEntityFrom(await GetQueryAsync<TEntity>());
        }

        public async Task<TEntity> FirstAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            return await criteria.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>()).FirstAsync();
        }

        public async Task<TEntity> FirstAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await (await GetQueryAsync<TEntity>()).FirstAsync(predicate);
        }

        public async Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return (await GetQueryAsync<TEntity>()).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
            }
            return (await GetQueryAsync<TEntity>()).OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        public async Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return (await GetQueryAsync(criteria)).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
            }
            return (await GetQueryAsync(criteria)).OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        public async Task<IEnumerable<TEntity>> GetAsync<TOrderBy, TEntity>(ISpecification<TEntity> specification, Expression<Func<TEntity, TOrderBy>> orderBy, int pageIndex, int pageSize, SortOrder sortOrder = SortOrder.Ascending) where TEntity : class
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return specification.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>()).OrderBy(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
            }
            return specification.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>()).OrderByDescending(orderBy).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsEnumerable();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>() where TEntity : class
        {
            return (await GetQueryAsync<TEntity>()).AsEnumerable();
        }

        public async Task<TEntity> GetByKeyAsync<TEntity>(object keyValue) where TEntity : class
        {
            return await DbContext.Set<TEntity>().FindAsync(keyValue);
        }

        public async Task<IQueryable<TEntity>> GetQueryAsync<TEntity>(ISpecification<TEntity> criteria, bool forUpdate = false) where TEntity : class
        {
            if (!forUpdate)
                return criteria.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>());
            else
                return criteria.SatisfyingEntitiesFrom(await GetQueryAsync<TEntity>(true));

        }

        public async Task<IQueryable<TEntity>> GetQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, bool forUpdate = false) where TEntity : class
        {
            if (!forUpdate)
                return (await GetQueryAsync<TEntity>()).Where(predicate);
            else
                return (await GetQueryAsync<TEntity>(true)).Where(predicate);

        }
        public async Task<IQueryable<TEntity>> GetQueryAsync<TEntity>(bool forUpdate = false) where TEntity : class
        {
            IQueryable<TEntity> query;
            if (forUpdate)
                query = DbContext.Set<TEntity>();
            else
            {
                query = DbContext.Set<TEntity>().AsNoTracking();
            }
            return query;
        }



        public async Task<TEntity> SingleAsync<TEntity>(Expression<Func<TEntity, bool>> criteria) where TEntity : class
        {
            return (await GetQueryAsync<TEntity>()).Single(criteria);
        }

        public async Task<TEntity> SingleAsync<TEntity>(ISpecification<TEntity> criteria) where TEntity : class
        {
            return criteria.SatisfyingEntityFrom(await GetQueryAsync<TEntity>());
        }

        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {

            var keyName = DbContext.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();
            var keyValue = entity.GetType().GetProperty(keyName).GetValue(entity, null);

            var attachedObject = DbContext.ChangeTracker
                .Entries<TEntity>().FirstOrDefault(x => x.Metadata.FindPrimaryKey().Properties.First(y => y.Name == keyName) == keyValue);
            if (attachedObject != null)
            {
                attachedObject.State = EntityState.Detached;
            }

            //DbContext.Entry(entity).Property("UpdatedOn").OriginalValue = DbContext.Entry(entity).Property("UpdatedOn").CurrentValue;
            //DbContext.Entry(entity).Property("UpdatedOn").CurrentValue = DateTime.Now;
            DbContext.Entry(entity).State = EntityState.Modified;
            DbContext.Set<TEntity>().Update(entity);
        }

        public DateTime SetKindUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified ?
                DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) :
                dateTime.ToUniversalTime(); // Ensure it's converted if needed
        }


        #region private 

        private string GetEntityName<TEntity>() where TEntity : class
        {
            //PluralizationService pluralizer = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en"));
            // return string.Format("{0}.{1}", ((IObjectContextAdapter)DbContext).ObjectContext.DefaultContainerName, pluralizer.Pluralize(typeof(TEntity).Name));

            // Thanks to Kamyar Paykhan -  http://huyrua.wordpress.com/2011/04/13/entity-framework-4-poco-repository-and-specification-pattern-upgraded-to-ef-4-1/#comment-688
            //string entitySetName = DbContext.MetadataWorkspace
            //    .GetEntityContainer(((IObjectContextAdapter)DbContext).ObjectContext.DefaultContainerName, DataSpace.CSpace)
            //                        .BaseEntitySets.Where(bes => bes.ElementType.Name == typeof(TEntity).Name).First().Name;

            string entitySetName = DbContext.Model.GetEntityTypes().First().Name;
            var mapping = DbContext.Model.FindEntityType(typeof(TEntity));
            //var schemaAttribute = mapping.ClrType.GetCustomAttributes<SchemaAttribute>(false).FirstOrDefault();
            //string schema = schemaAttribute?.Name;

            //var schema = mapping.GetSchema();
            //var tableName = mapping.GetTableName();

            return String.Format("{0}.{1}", "schema", "tableName");
        }

        public virtual int GetKey<T>(T entity)
        {
            var keyName = DbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            return (int)entity.GetType().GetProperty(keyName).GetValue(entity, null);
        }

        private DbContext DbContext
        {
            get
            {
                // if (this._context == null)
                // {
                //     if (this._connectionStringName == string.Empty)
                //         this._context = DbContextManager.Current;
                //     else
                //         this._context = DbContextManager.CurrentFor(this._connectionStringName);
                // }
                return this._context;
            }
        }


        private UnitOfWork unitOfWork;
        #endregion
    }
}
