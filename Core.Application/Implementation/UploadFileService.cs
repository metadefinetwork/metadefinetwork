using Core.Application.Interfaces;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.System;
using Core.Data.EF;
using Core.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class UploadFileService : IUploadFileService
    {
        private readonly AppDbContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IWalletMVRTransactionService _walletMVRTransactionService;
        private readonly IImportExcelService _importExcel;

        public UploadFileService(AppDbContext context,
                                 ITransactionService transactionService,
                                 IImportExcelService importExcel,
                                 IWalletMVRTransactionService walletMVRTransactionService)
        {
            _context = context;
            _transactionService = transactionService;
            _importExcel = importExcel;
            _walletMVRTransactionService = walletMVRTransactionService;
        }

        public async Task<(int sumUpdated, List<MemberBalanceModel> notFoundMember)> ProcessUpdateInvestBalanceForMembers(ImportExcelModel model)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                var notFoundUser = new List<MemberBalanceModel>();

                try
                {
                    var members = await _importExcel.ExtractMemberBalanceFromFile(model);
                    int count = 0;
                    foreach (var item in members)
                    {
                        var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == item.Email);

                        if (appUser == null)
                        {
                            notFoundUser.Add(item);
                            continue;
                        }

                        //appUser.InvestBalance += item.Balance;

                        var investTransaction = new WalletMVRTransactionViewModel
                        {
                            AppUserId = appUser.Id,
                            AddressFrom = "SYSTEM",
                            AddressTo = "Wallet Invest",
                            AmountReceive = item.Balance,
                            Amount = item.Balance,
                            Fee = 0,
                            FeeAmount = 0,
                            Type = WalletMVRTransactionType.Airdrop,
                            DateCreated = DateTime.Now,
                            TransactionHash = "SYSTEM"
                        };

                        _walletMVRTransactionService.Add(investTransaction);
                        count++;
                    }

                    await _context.SaveChangesAsync();

                    transaction.Commit();

                    return (count, notFoundUser);
                }
                catch (Exception e)
                {
                    transaction.Rollback();

                    throw e;
                }
            }
        }
    }
}
