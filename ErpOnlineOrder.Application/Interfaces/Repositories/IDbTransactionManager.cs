using System;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Repositories
{
    public interface ITransaction : IDisposable, IAsyncDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
    public interface IDbTransactionManager
    {
        Task<ITransaction> BeginTransactionAsync();
    }
}