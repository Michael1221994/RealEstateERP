using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture.Base.Specifications.Extensions
{
    public abstract class CompositeSpecification<TEntity> : ISpecification<TEntity>
    {
        protected readonly Specification<TEntity> _leftSide;

        protected readonly Specification<TEntity> _rightSide;

        public CompositeSpecification(Specification<TEntity> leftSide, Specification<TEntity> rightSide)
        {
            _leftSide = leftSide;
            _rightSide = rightSide;
        }

        public abstract TEntity SatisfyingEntityFrom(IQueryable<TEntity> query);

        public abstract IQueryable<TEntity> SatisfyingEntitiesFrom(IQueryable<TEntity> query);
    }
}
