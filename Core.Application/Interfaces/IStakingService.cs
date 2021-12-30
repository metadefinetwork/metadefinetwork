using Core.Application.ViewModels.Fishing;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Utilities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Application.Interfaces
{
    public interface IStakingService
    {
        PagedResult<StakingViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize);

        decimal GetTotalPackage(Guid userId, StakingType? type);

        decimal GetMaxPackage(Guid userId);

        StakingViewModel GetById(int id);

        void Update(StakingViewModel model);

        void Add(StakingViewModel Model);

        void Save();

        PagedResult<ItemViewModel> GetItemAllPaging(ItemListingRequest request);

        PagedResult<MyItemViewModel> GetMyItemAllPaging(MyItemListingRequest request);
        PagedResult<HistoryRewardViewModel> GetHistoryRewardAllPaging(HistoryRewardListingRequest request);
    }
}
