using Core.Application.ViewModels.System;
using Core.Utilities.Dtos;
using System;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IWalletMARTransactionService
    {
        Task<PagedResult<WalletMARTransactionViewModel>> GetAllPagingSync(string keyword, string appUserId, int pageIndex, int pageSize);
        void Add(WalletMARTransactionViewModel Model);
        void Save();
    }
}
