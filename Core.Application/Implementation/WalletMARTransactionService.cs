using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class WalletMARTransactionService : IWalletMARTransactionService
    {

        private readonly IWalletMARTransactionRepository _walletMARTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        public WalletMARTransactionService(
          IWalletMARTransactionRepository walletMARTransactionRepository,
          IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _walletMARTransactionRepository = walletMARTransactionRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<PagedResult<WalletMARTransactionViewModel>> GetAllPagingSync(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _walletMARTransactionRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.TransactionHash.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.UserName.Contains(keyword)
                || x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword));

            if (!string.IsNullOrEmpty(appUserId))
            {
                query = query.Where(x => x.AppUserId == new Guid(appUserId));
            }

            var totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new WalletMARTransactionViewModel()
                {
                    Id = x.Id,
                    AddressFrom = x.AddressFrom,
                    AddressTo = x.AddressTo,
                    Fee = x.Fee,
                    FeeAmount = x.FeeAmount,
                    AmountReceive = x.AmountReceive,
                    Amount = x.Amount,
                    AppUserId = x.AppUserId,
                    AppUserName = x.AppUser.UserName,
                    Email = x.AppUser.Email,
                    Sponsor = $"MTD{x.AppUser.Sponsor}",
                    DateCreated = x.DateCreated,
                    TransactionHash = x.TransactionHash,
                    Type = x.Type,
                    TypeName = x.Type.GetDescription()
                }).ToList();

            return new PagedResult<WalletMARTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(WalletMARTransactionViewModel model)
        {
            var transaction = new WalletMARTransaction()
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

            _walletMARTransactionRepository.Add(transaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
