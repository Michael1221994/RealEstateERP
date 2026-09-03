using System.Linq;

namespace Infrastracture.Base.Specifications
{
    public interface ISpecification<TEntity>
    {
        TEntity SatisfyingEntityFrom(IQueryable<TEntity> query);

        IQueryable<TEntity> SatisfyingEntitiesFrom(IQueryable<TEntity> query);
    }
}
