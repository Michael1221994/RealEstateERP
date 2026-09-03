using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastracture.Base.EF
{
    public class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction _transaction;
        private DbContext _dbContext;

        public UnitOfWork(DbContext context)
        {
            _dbContext = context;
        }

        public bool IsInTransaction
        {
            get { return _transaction != null; }
        }

        public async Task BeginTransaction()
        {
            await BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public async Task BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
            {
                throw new ApplicationException("Cannot begin a new transaction while an existing transaction is still running. " +
                                                "Please commit or rollback the existing transaction before starting a new one.");
            }
            OpenConnection();
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task RollBackTransaction()
        {
            if (_transaction == null)
            {
                throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
            }

            if (IsInTransaction)
            {
                await _transaction.RollbackAsync();
                ReleaseCurrentTransaction();
            }
        }

        public async Task CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new ApplicationException("Cannot roll back a transaction while there is no transaction running.");
            }

            try
            {
                await _dbContext.SaveChangesAsync();
                _transaction.Commit();
                ReleaseCurrentTransaction();
            }
            catch
            {
                await RollBackTransaction();
                throw;
            }
        }

        public async Task SaveChanges()
        {
            try
            {
                if (IsInTransaction)
                {
                    throw new ApplicationException("A transaction is running. Call CommitTransaction instead.");
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    var proposedValues = entry.CurrentValues;
                    var databaseValues = entry.GetDatabaseValues();
                    var originalValues = entry.OriginalValues;

                    foreach (var property in proposedValues.Properties)
                    {
                        if (property.Name == "UpdatedOn")
                        {
                            var proposedValue = proposedValues[property];
                            var databaseValue = databaseValues[property];
                            var originalValue = originalValues[property];
                        }

                        // TODO: decide which value should be written to database
                        // proposedValues[property] = <value to be saved>;
                    }

                    // Refresh original values to bypass next concurrency check
                    // entry.OriginalValues.SetValues(databaseValues);
                }

                throw new ApplicationException("A Concurrency error occurred.");
            }
        }

        public async Task SaveChanges(SaveOptions saveOptions)
        {
            try
            {
                if (IsInTransaction)
                {
                    throw new ApplicationException("A transaction is running. Call CommitTransaction instead.");
                }

                await _dbContext.SaveChangesAsync(); ;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ApplicationException("A Concurrency error occurred.");
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off the managed and unmanaged resources used.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            if (_disposed)
                return;

            _disposed = true;
        }

        private bool _disposed;
        #endregion

        private void OpenConnection()
        {
            _dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// Releases the current transaction
        /// </summary>
        private void ReleaseCurrentTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

    }
}
