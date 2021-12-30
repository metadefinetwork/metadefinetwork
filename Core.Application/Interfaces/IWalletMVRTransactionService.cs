using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface IWalletMVRTransactionService
    {
        PagedResult<WalletMVRTransactionViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        void Add(WalletMVRTransactionViewModel Model);

        void Save();
    }
}
