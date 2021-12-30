using Core.Application.Interfaces;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class LuckyRoundHistoryService : ILuckyRoundHistoryService
    {
        private readonly ILuckyRoundHistoryRepository _luckyRoundHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LuckyRoundHistoryService(
            ILuckyRoundHistoryRepository luckyRoundHistoryRepository,
            IUnitOfWork unitOfWork)
        {
            _luckyRoundHistoryRepository = luckyRoundHistoryRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<LuckyRoundHistoryViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _luckyRoundHistoryRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Value.ToString().Contains(keyword));

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new LuckyRoundHistoryViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Amount = x.Amount,
                    Sponsor = $"{x.AppUser.Sponsor}",
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.Email,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Unit = x.Unit,
                    UnitName = x.Unit.GetDescription(),
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated
                }).ToList();

            return new PagedResult<LuckyRoundHistoryViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public int CountByType(LuckyRoundHistoryType type, int luckyRoundId)
        {
            int countItem = _luckyRoundHistoryRepository
                .FindAll(x => x.Type == type && x.LuckyRoundId == luckyRoundId).Count();
            return countItem;
        }

        public void Add(LuckyRoundHistoryViewModel Vm)
        {
            var model = new LuckyRoundHistory()
            {
                Id = Vm.Id,
                DateCreated = Vm.DateCreated,
                DateUpdated = Vm.DateUpdated,
                AddressFrom = Vm.AddressFrom,
                AddressTo = Vm.AddressTo,
                Amount = Vm.Amount,
                AppUserId = Vm.AppUserId,
                LuckyRoundId = Vm.LuckyRoundId,
                Unit = Vm.Unit,
                Type = Vm.Type
            };

            _luckyRoundHistoryRepository.Add(model);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
