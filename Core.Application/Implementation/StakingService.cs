using Core.Application.Interfaces;
using Core.Application.ViewModels.Fishing;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class StakingService
        : IStakingService
    {
        private readonly IStakingRepository _stakingRepository;
        private readonly IStakingRewardRepository _stakingRewardRepository;
        private readonly IItemGameUserRepository _itemGameUserRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IItemGameRepository _itemGameRepository;

        public StakingService(
            IStakingRepository stakingRepository,
            IStakingRewardRepository stakingRewardRepository,
            IItemGameUserRepository itemGameUserRepository,
            IItemGameRepository itemGameRepository,
            IUnitOfWork unitOfWork)
        {
            _stakingRepository = stakingRepository;
            _stakingRewardRepository = stakingRewardRepository;
            _itemGameUserRepository = itemGameUserRepository;
            _itemGameRepository = itemGameRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<GenericResult> ProcessStakingAsync(Staking item)
        {


            return GenericResult.ToSuccess();
        }

        public PagedResult<StakingViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _stakingRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Value.ToString().Contains(keyword));

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new StakingViewModel()
                {
                    //Id = x.Id,
                    //IsGetedCommission = x.ReceiveLatest.Date == DateTime.Now.Date,
                    //Package = x.Package,
                    //PackageName = x.Package.GetDescription(),
                    //InterestRate = x.InterestRate,
                    //ReceiveAmount = x.ReceiveAmount,
                    //ReceiveLatest = x.ReceiveLatest,
                    //ReceiveTimes = x.ReceiveTimes,
                    //StakingAmount = x.StakingAmount,
                    //StakingTimes = x.StakingTimes,
                    //TimeLine = x.TimeLine,
                    //TimeLineName = x.TimeLine.GetDescription(),
                    //AppUserId = x.AppUserId,
                    //AppUserName = x.AppUser.UserName,
                    //DateCreated = x.DateCreated,
                    //Type = x.Type,
                    //TypeName = x.Type.GetDescription()
                }).ToList();

            return new PagedResult<StakingViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public decimal GetTotalPackage(Guid userId, StakingType? type)
        {
            decimal totalStaking = 0;
            var staking = _stakingRepository.FindAll(x => x.AppUserId == userId);

            //if (type != null)
            //    staking = staking.Where(x => x.Type == type);

            //totalStaking = staking.Sum(x => x.StakingAmount);

            return totalStaking;
        }

        public decimal GetMaxPackage(Guid userId)
        {
            //decimal maxPackage = _stakingRepository.FindAll(x => x.AppUserId == userId)
            //    .Max(x => x.StakingAmount);

            //return maxPackage;

            return 0m;
        }

        public StakingViewModel GetById(int id)
        {
            var model = _stakingRepository.FindById(id);
            if (model == null)
                return null;

            var staking = new StakingViewModel()
            {
                //Id = model.Id,
                //Package = model.Package,
                //PackageName = model.Package.GetDescription(),
                //InterestRate = model.InterestRate,
                //ReceiveAmount = model.ReceiveAmount,
                //ReceiveLatest = model.ReceiveLatest,
                //ReceiveTimes = model.ReceiveTimes,
                //StakingAmount = model.StakingAmount,
                //StakingTimes = model.StakingTimes,
                //TimeLine = model.TimeLine,
                //TimeLineName = model.TimeLine.GetDescription(),
                //AppUserId = model.AppUserId,
                //AppUserName = model.AppUser.UserName,
                //DateCreated = model.DateCreated,
                //Type = model.Type,
                //TypeName = model.Type.GetDescription()
            };

            return staking;
        }

        public void Update(StakingViewModel model)
        {
            var staking = _stakingRepository.FindById(model.Id);

            //staking.Package = model.Package;
            //staking.InterestRate = model.InterestRate;
            //staking.TimeLine = model.TimeLine;
            //staking.StakingTimes = model.StakingTimes;
            //staking.StakingAmount = model.StakingAmount;
            //staking.ReceiveTimes = model.ReceiveTimes;
            //staking.ReceiveAmount = model.ReceiveAmount;
            //staking.ReceiveLatest = model.ReceiveLatest;
            //staking.AppUserId = model.AppUserId;
            //staking.DateCreated = model.DateCreated;
            //staking.Type = model.Type;

            _stakingRepository.Update(staking);
        }

        public void Add(StakingViewModel model)
        {
            var transaction = new Staking()
            {
                //Package = model.Package,
                //InterestRate = model.InterestRate,
                //TimeLine = model.TimeLine,
                //StakingTimes = model.StakingTimes,
                //StakingAmount = model.StakingAmount,
                //ReceiveTimes = model.ReceiveTimes,
                //ReceiveAmount = model.ReceiveAmount,
                //ReceiveLatest = model.ReceiveLatest,
                //AppUserId = model.AppUserId,
                //DateCreated = model.DateCreated,
                //Type = model.Type
            };

            _stakingRepository.Add(transaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public PagedResult<ItemViewModel> GetItemAllPaging(ItemListingRequest request)
        {
            var query = _itemGameRepository.FindAll(x => x.Status == Status.Active);

            if (!string.IsNullOrEmpty(request.KeyWord))
                query = query.Where(x => x.Name.Contains(request.KeyWord));

            if (request.Type != -1)
                query = query.Where(x => x.Type == (ItemType)request.Type);


            if (request.Group != -1)
                query = query.Where(x => x.GroupItem == (ItemGroup)request.Group);

            if (request.Price != 0m)
                query = query.Where(x => x.Price > request.Price);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Type)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                .Select(x => new ItemViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    DateCreated = x.DateCreated,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Status = x.Status,
                    StatusName = x.Status.GetDescription(),
                    ClassName = x.ClassName,
                    GroupItem = x.GroupItem,
                    GroupItemName = x.GroupItem.GetDescription()
                }).ToList();

            return new PagedResult<ItemViewModel>()
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public PagedResult<MyItemViewModel> GetMyItemAllPaging(MyItemListingRequest request)
        {
            var result = new List<MyItemViewModel>();
            var totalRow = 0;
            if (request.GroupId == (int)ItemGroup.Staking)
            {

                var query = _stakingRepository.FindAll(x => x.AppUser).Where(x => x.AppUserId.ToString() == request.AppUserId);
                totalRow = query.Count();
                result = query.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                    .Select(x => new MyItemViewModel()
                    {
                        Id = x.Id,
                        AppUserId = x.AppUserId,
                        DateCreated = x.DateCreated,
                        Interest = x.InterestRate,
                        ItemStakingId = x.ItemGameId,
                        Price = x.Amount,
                        LastReceived = x.LastReceived,
                        StakingTimeLineId = x.StakingPeriodId,
                        Status = x.Status,
                        //DateExpired = x.DateCreated.AddDays(x.Period * 30).ToString("MM/dd/yyyy"), 
                        StatusName = x.Status.GetDescription(),
                        ItemStaking = new ItemViewModel
                        {
                            Id = x.ItemGame.Id,
                            Name = x.ItemGame.Name,
                            Price = x.ItemGame.Price,
                            DateCreated = x.ItemGame.DateCreated,
                            Type = x.ItemGame.Type,
                            TypeName = x.ItemGame.Type.GetDescription(),
                            Status = x.ItemGame.Status,
                            StatusName = x.ItemGame.Status.GetDescription(),
                            ClassName = x.ItemGame.ClassName,
                            GroupItem = x.ItemGame.GroupItem,
                            GroupItemName = x.ItemGame.GroupItem.GetDescription()
                        }
                    }).ToList();
            }
            else if(request.GroupId == (int)ItemGroup.Game) {
                var query = _itemGameUserRepository.FindAll(x => x.AppUser).Where(x => x.AppUserId.ToString() == request.AppUserId);
                totalRow = query.Count();
                result = query.OrderByDescending(x => x.Id)
                    .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                    .Select(x => new MyItemViewModel()
                    {
                        Id = x.Id,
                        AppUserId = x.AppUserId,
                        DateCreated = x.DateCreated,
                        ItemStakingId = x.ItemGameId,
                        Price = x.Amount,
                        StatusItemGame = x.Status,
                        StatusItemGameName = x.Status.GetDescription(),
                        ItemStaking = new ItemViewModel
                        {
                            Id = x.ItemGame.Id,
                            Name = x.ItemGame.Name,
                            Price = x.ItemGame.Price,
                            DateCreated = x.ItemGame.DateCreated,
                            Type = x.ItemGame.Type,
                            TypeName = x.ItemGame.Type.GetDescription(),
                            Status = x.ItemGame.Status,
                            StatusName = x.ItemGame.Status.GetDescription(),
                            ClassName = x.ItemGame.ClassName,
                            GroupItem = x.ItemGame.GroupItem,
                            GroupItemName = x.ItemGame.GroupItem.GetDescription()
                        }
                    }).ToList();
            }
            return new PagedResult<MyItemViewModel>()
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                Results = result,
                RowCount = totalRow
            };

        }

        public PagedResult<HistoryRewardViewModel> GetHistoryRewardAllPaging(HistoryRewardListingRequest request)
        {
            var result = new List<HistoryRewardViewModel>();
            var totalRow = 0;

            var query = _stakingRewardRepository.FindAll(x => x.AppUser).Where(x => x.AppUserId.ToString() == request.AppUserId);
            totalRow = query.Count();
            result = query.OrderByDescending(x => x.Id)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                .Select(x => new HistoryRewardViewModel()
                {
                    AppUserId = x.AppUserId,
                    DateCreated = x.DateCreated,
                    InterestRate = x.InterestRate,
                    ReceivedAmount = x.ReceivedAmount,
                    StakingId = x.StakingId,
                    Amount = x.Amount,
                    ItemInfo = new ItemViewModel
                    {
                        Name = x.Staking.ItemGame.Name,
                    }
                }).ToList();
            return new PagedResult<HistoryRewardViewModel>()
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                Results = result,
                RowCount = totalRow
            };
        }
    }
}
