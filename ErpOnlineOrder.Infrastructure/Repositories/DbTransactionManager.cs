using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Infrastructure.Repositories
{
    public class EfTransaction : ITransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public async Task CommitAsync() => await _transaction.CommitAsync();
        public async Task RollbackAsync() => await _transaction.RollbackAsync();
        public void Dispose() => _transaction.Dispose();
        public async ValueTask DisposeAsync() => await _transaction.DisposeAsync();
    }

    public class DbTransactionManager : IDbTransactionManager
    {
        private readonly ErpOnlineOrderDbContext _context;
        public DbTransactionManager(ErpOnlineOrderDbContext context)
        {
            _context = context;
        }

        public async Task<ITransaction> BeginTransactionAsync()
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            return new EfTransaction(transaction);
        }
    }
}