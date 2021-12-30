using Core.Application.ViewModels.Dapp;
using Core.Data.Entities;
using Core.Utilities.Dtos;
using Core.Utilities.Dtos.Datatables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Interfaces
{
    public interface IDappService
    {
        decimal ConculateTokenAmount(decimal bnbAmount, decimal priceBNBBep20);
        Task ProcessCommission(decimal amount, Guid userRef, Guid userAdmin, int level = 1);
        Task<PagedResult<DAppTransaction>> GetTransactionsAsync(string key, int pageIndex, int pageSize, string type, string userId);
        Task<(GenericResult result, string transactionId)> InitializeTransactionProgress(DappInitializationParams model, string type);
        Task<GenericResult> ProcessVerificationTransaction(string transactionHash, string tempDappTransaction, bool isRetry);

        Task<PagedResult<DAppTransactionView>> GetTransactionsAsync(string keyword, int pageIndex, int pageSize);
        Task<bool> HasClaim(string email);
    }
}
