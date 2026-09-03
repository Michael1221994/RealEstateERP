using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastracture.Base
{
    public interface IUnitOfWork
    {
        bool IsInTransaction { get; }

        Task SaveChanges();

        Task SaveChanges(SaveOptions saveOptions);

        Task BeginTransaction();

        Task BeginTransaction(IsolationLevel isolationLevel);

        Task RollBackTransaction();

        Task CommitTransaction();
    }
}
