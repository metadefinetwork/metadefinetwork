using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface ITransactionService
    {
        PagedResult<TransactionViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        decimal GetUserAmountByType(Guid appUserId, TransactionType type);

        int CountByType(string appUserId, TransactionType type);

        int CountByType(TransactionType type);

        void Add(TransactionViewModel Model);

        void Save();
    }
}
