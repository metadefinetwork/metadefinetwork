using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class TicketTransactionService : ITicketTransactionService
    {
        private readonly IWalletMARTransactionService _walletMARTransactionService;
        private readonly ITicketTransactionRepository _ticketTransactionRepository;
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public TicketTransactionService(
            IWalletMARTransactionService walletMARTransactionService,
            UserManager<AppUser> userManager,
            IBlockChainService blockChainService,
            ITicketTransactionRepository ticketTransactionRepository,
            IUnitOfWork unitOfWork)
        {
            _walletMARTransactionService = walletMARTransactionService;
            _blockChainService = blockChainService;
            _userManager = userManager;
            _ticketTransactionRepository = ticketTransactionRepository;
            _unitOfWork = unitOfWork;
        }

        public PagedResult<TicketTransactionViewModel> GetAllPaging(string keyword, string appUserId, int pageIndex, int pageSize)
        {
            var query = _ticketTransactionRepository.FindAll(x => x.AppUser);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.AddressFrom.Contains(keyword)
                || x.AddressTo.Contains(keyword)
                || x.AppUser.Email.Contains(keyword)
                || x.AppUser.Sponsor.Value.ToString().Contains(keyword));

            if (!string.IsNullOrWhiteSpace(appUserId))
                query = query.Where(x => x.AppUserId.ToString() == appUserId);

            var totalRow = query.Count();
            var data = query.OrderBy(x => x.Status).Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(x => new TicketTransactionViewModel()
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
                    Sponsor = $"MTD{ x.AppUser.Sponsor}",
                    Type = x.Type,
                    TypeName = x.Type.GetDescription(),
                    Status = x.Status,
                    StatusName = x.Status.GetDescription(),
                    Unit = x.Unit,
                    UnitName = x.Unit.GetDescription(),
                    DateCreated = x.DateCreated,
                    DateUpdated = x.DateUpdated
                }).ToList();

            return new PagedResult<TicketTransactionViewModel>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        public void Add(TicketTransactionViewModel model)
        {
            var transaction = new TicketTransaction()
            {
                AddressFrom = model.AddressFrom,
                AddressTo = model.AddressTo,

                Fee = model.Fee,
                FeeAmount = model.FeeAmount,
                AmountReceive = model.AmountReceive,
                Amount = model.Amount,

                AppUserId = model.AppUserId,
                Unit = model.Unit,
                Status = model.Status,
                Type = model.Type,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };

            _ticketTransactionRepository.Add(transaction);
        }

        public async Task Rejected(int id)
        {
            var ticketTransaction = _ticketTransactionRepository.FindById(id);

            ticketTransaction.Status = TicketTransactionStatus.Rejected;
            ticketTransaction.DateUpdated = DateTime.Now;
            _ticketTransactionRepository.Update(ticketTransaction);
            _unitOfWork.Commit();

            var appUser = await _userManager.FindByIdAsync(ticketTransaction.AppUserId.ToString());

            if (ticketTransaction.Type == TicketTransactionType.WithdrawMAR)
            {
                appUser.MARBalance += ticketTransaction.Amount;
                await _userManager.UpdateAsync(appUser);
            }

        }

        public async Task<GenericResult> Approved(int id)
        {
            var ticketTransaction = _ticketTransactionRepository.FindById(id);

            if (ticketTransaction.Type == TicketTransactionType.WithdrawMAR)
            {
                decimal balanceTransfer = ticketTransaction.AmountReceive;

                var tokenTransaction = await _blockChainService
                        .SendERC20Async(CommonConstants.BEP20EXCHANGEPrKey,
                        ticketTransaction.AddressTo,
                        CommonConstants.BEP20TokenContract, balanceTransfer,
                        CommonConstants.BEP20TokenDP, CommonConstants.BEP20Url);

                if (tokenTransaction.Succeeded(true))
                {
                    var walletMARTransaction = new WalletMARTransactionViewModel
                    {
                        AppUserId = ticketTransaction.AppUserId,
                        AddressFrom = CommonConstants.BEP20EXCHANGEPuKey,
                        AddressTo = ticketTransaction.AddressTo,

                        Amount = ticketTransaction.Amount,
                        FeeAmount = ticketTransaction.FeeAmount,
                        Fee = ticketTransaction.Fee,
                        AmountReceive = ticketTransaction.AmountReceive,

                        TransactionHash = tokenTransaction.TransactionHash,
                        Type = WalletMARTransactionType.Withdraw,
                        DateCreated = DateTime.Now
                    };

                    _walletMARTransactionService.Add(walletMARTransaction);
                    _walletMARTransactionService.Save();
                }
            }
            else
            {

            }

            ticketTransaction.Status = TicketTransactionStatus.Approved;
            ticketTransaction.DateUpdated = DateTime.Now;
            _ticketTransactionRepository.Update(ticketTransaction);
            _unitOfWork.Commit();

            return new GenericResult(true, "Approve ticket is success", ticketTransaction);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
