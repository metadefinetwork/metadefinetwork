using Core.Application.Interfaces;
using Core.Application.ViewModels.Report;
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IDAppTransactionRepository _metamaskTransaction;
        private readonly ITicketTransactionRepository _ticketTransactionRepository;
        private readonly IWalletBNBTransactionRepository _walletBNBTransactionRepository;
        private readonly IWalletMVRTransactionRepository _walletMVRTransactionRepository;
        private readonly IWalletMARTransactionRepository _walletMARTransactionRepository;
        private readonly IBlockChainService _blockChainService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public ReportService(IDAppTransactionRepository metamaskTransaction,
                             ITicketTransactionRepository ticketTransactionRepository,
                             IWalletBNBTransactionRepository walletBNBTransactionRepository,
                             IWalletMVRTransactionRepository walletMVRTransactionRepository,
                             IWalletMARTransactionRepository walletMARTransactionRepository,
                             IBlockChainService blockChainService,
                             IUnitOfWork unitOfWork,
                             UserManager<AppUser> userManager)
        {
            _metamaskTransaction = metamaskTransaction;
            _ticketTransactionRepository = ticketTransactionRepository;
            _walletBNBTransactionRepository = walletBNBTransactionRepository;
            _walletMVRTransactionRepository = walletMVRTransactionRepository;
            _walletMARTransactionRepository = walletMARTransactionRepository;
            _blockChainService = blockChainService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public ReportViewModel GetReportInfo(string startDate, string endDate)
        {
            DateTime now = DateTime.Now.Date;

            var appUsers = _userManager.Users;

            var model = new ReportViewModel();
            model.TotalMember = appUsers.Count();
            model.TodayMember = appUsers.Count(x => x.DateCreated.Date == now);
            model.TotalMemberInVerifyEmail = appUsers.Count(x => x.EmailConfirmed == false);
            model.TotalMemberVerifyEmail = appUsers.Count(x => x.EmailConfirmed == true);

            #region BNB

            var bnbTransactions = _walletBNBTransactionRepository.FindAll();

            model.TodayBNBDeposit = bnbTransactions
                .Where(x => x.Type == WalletBNBTransactionType.Deposit
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            model.TodayBNBWithdraw = bnbTransactions
                .Where(x => x.Type == WalletBNBTransactionType.Withdraw
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                bnbTransactions = bnbTransactions.Where(x => x.DateCreated.Date >= start);
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                bnbTransactions = bnbTransactions.Where(x => x.DateCreated.Date <= end);
            }

            model.TotalBNBDeposit = bnbTransactions
                .Where(x => x.Type == WalletBNBTransactionType.Deposit).Sum(x => x.Amount);

            model.TotalBNBWithdraw = bnbTransactions
                .Where(x => x.Type == WalletBNBTransactionType.Withdraw).Sum(x => x.Amount);

            #endregion BNB

            #region MVR

            var mvrTransactions = _walletMVRTransactionRepository.FindAll();

            model.TodayMVRDeposit = mvrTransactions
                .Where(x => x.Type == WalletMVRTransactionType.Deposit
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            model.TodayMVRWithdraw = mvrTransactions
                .Where(x => x.Type == WalletMVRTransactionType.Withdraw
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                mvrTransactions = mvrTransactions.Where(x => x.DateCreated.Date >= start);
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                mvrTransactions = mvrTransactions.Where(x => x.DateCreated.Date <= end);
            }

            model.TotalMVRDeposit = mvrTransactions
                .Where(x => x.Type == WalletMVRTransactionType.Deposit).Sum(x => x.Amount);

            model.TotalMVRWithdraw = mvrTransactions
                .Where(x => x.Type == WalletMVRTransactionType.Withdraw).Sum(x => x.Amount);

            #endregion Invest

            #region MAR

            var marTransactions = _walletMARTransactionRepository.FindAll();

            model.TodayMARDeposit = marTransactions
                .Where(x => x.Type == WalletMARTransactionType.Deposit
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            model.TodayMARWithdraw = marTransactions
                .Where(x => x.Type == WalletMARTransactionType.Withdraw
                && x.DateCreated.Date == now).Sum(x => x.Amount);

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                DateTime start = DateTime.Parse(startDate);
                marTransactions = marTransactions.Where(x => x.DateCreated.Date >= start);
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                DateTime end = DateTime.Parse(endDate);
                marTransactions = marTransactions.Where(x => x.DateCreated.Date <= end);
            }

            model.TotalMARDeposit = marTransactions
                .Where(x => x.Type == WalletMARTransactionType.Deposit).Sum(x => x.Amount);

            model.TotalMARWithdraw = marTransactions
                .Where(x => x.Type == WalletMARTransactionType.Withdraw).Sum(x => x.Amount);

            #endregion MAR

            return model;
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }
    }
}
