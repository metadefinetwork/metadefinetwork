using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using System;
using System.Linq;

namespace Core.Application.Implementation
{
    public class WalletBNBTransactionService : IWalletBNBTransactionService
    {
        private readonly IWalletBNBTransactionRepository _walletBNBTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WalletBNBTransactionService(
            IWalletBNBTransactionRepository walletBNBTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _walletBNBTransactionRepository = walletBNBTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<WalletBNBTransactionViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _walletBNBTransactionRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Value.ToString().Contains(keyword));

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletBNBTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,

                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,

                    StrAmount = x.Amount.ToString(),
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription()
                }).ToList();

            return new PagedResult<WalletBNBTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(WalletBNBTransactionViewModel model)
        {
            var transaction = new WalletBNBTransaction()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,

                Fee = model.Fee,
                FeeAmount = model.FeeAmount,
                AmountReceive = model.AmountReceive,
                Amount = model.Amount,

                AppUserId = model.AppUserId,
                DateCreated = model.DateCreated,
                TransactionHash = model.TransactionHash,
                Type = model.Type
            };

            _walletBNBTransactionRepository.Add(transaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
