using Core.Application.Filters;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Dtos.Datatables;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class DappService : BaseService, IDappService
    {
        private readonly IRepository<DAppTransaction, Guid> _dappRepository;
        private readonly IRepository<WalletMARTransaction, int> _marTransactinoRepository;
        private readonly IBlockChainService _blockChain;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DappService> _logger;
        private readonly IRepository<QueueTask, int> _queueRepository;
        private readonly TelegramBotWrapper _botTelegramService;
        public const string DAppTransactionId = "DAppTransactionId";

        private static AsyncRetryPolicy _policy = Policy
          .Handle<Exception>()
          .WaitAndRetryAsync(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(4),
                TimeSpan.FromSeconds(8),
                TimeSpan.FromSeconds(15)
              });

        public DappService(UserManager<AppUser> userManager,
                           IRepository<DAppTransaction, Guid> dappRepository,
                           IBlockChainService blockChain,
                           IUnitOfWork unitOfWork,
                           ILogger<DappService> logger,
                           IRepository<WalletMARTransaction, int> marTransactinoRepository,
                           IRepository<QueueTask, int> queueRepository,
                           TelegramBotWrapper botTelegramService) : base(userManager)
        {
            _dappRepository = dappRepository;
            _blockChain = blockChain;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _marTransactinoRepository = marTransactinoRepository;
            _queueRepository = queueRepository;
            _botTelegramService = botTelegramService;
        }

        public async Task<(GenericResult result, string transactionId)> InitializeTransactionProgress(DappInitializationParams model, string type)
        {

            var validateResult = type switch
            {
                "presale" => await ValidateBuyTokenParams(model),
                "claim" => await ValidateClaimTokenParams(model),
                _ => new GenericResult(false, "No type was found")
            };

            if (!validateResult.Success)
            {
                return (validateResult, string.Empty);
            }

            decimal priceBNBBep20 = _blockChain.GetCurrentPrice("BNB", "USD");

            if (priceBNBBep20 == 0)
                return (new GenericResult(false, "There is a problem loading the currency value!"), string.Empty);

            var transaction = new DAppTransaction()
            {
                Id = Guid.NewGuid(),
                AddressFrom = model.Address,
                AddressTo = CommonConstants.BEP20SYSTEMPuKey,
                DateCreated = DateTime.Now,
                DAppTransactionState = DAppTransactionState.Requested,
                IsDevice = model.IsDevice,
                WalletType = model.WalletType,
                Email = model.Email
            };

            switch (type)
            {
                case "presale":
                    transaction.TokenAmount = ConculateTokenAmount(model.BNBAmount, priceBNBBep20);
                    transaction.Type = DAppTransactionType.Presale;
                    transaction.BNBAmount = model.BNBAmount;
                    break;
                case "claim":
                    transaction.TokenAmount = 50000m;
                    transaction.Type = DAppTransactionType.Claim;
                    transaction.BNBAmount = Math.Round(20 / priceBNBBep20, 4);
                    break;
            }

            await _dappRepository.AddAsync(transaction);
            _unitOfWork.Commit();

            var convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();

            var message = new BlockchainParams
            {
                TransactionHex = convertor.ConvertToHex(transaction.Id.ToString()),
                From = model.Address,
                To = CommonConstants.BEP20SYSTEMPuKey,
                Value = transaction.BNBAmount,
                Gas = "0x55f0",//22000
                GasPrice = "0x2540be400"
            };

            return (GenericResult.ToSuccess("Successed to initialize Transaction", message), transaction.Id.ToString());
        }

        public async Task<GenericResult> ProcessVerificationTransaction(string transactionHash, string tempDappTransaction, bool isRetry = false)
        {
            try
            {
                _logger.LogInformation($"Start calling VerifyMetaMaskRequest with transaction hash: {transactionHash}");

                var transactionReceipt = await _policy.ExecuteAsync<TransactionReceipt>(async () =>
                {
                    var result = await _blockChain.GetTransactionReceiptByTransactionID(transactionHash, CommonConstants.BEP20Url);
                    if (result == null)
                    {
                        _logger.LogInformation("retry get receipt of transaction hash: {0}", transactionHash);

                        throw new ArgumentNullException($"Cannot GetTransactionReceipt By {transactionHash}");
                    }

                    return result;
                });

                var transaction = await _blockChain.GetTransactionByTransactionID(transactionHash, CommonConstants.BEP20Url);

                var uft8Convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();
                var transactionId = uft8Convertor.ConvertFromHex(transaction.Input);

                var dappTransaction = _dappRepository.FindById(Guid.Parse(transactionId));

                var balance = Web3.Convert.FromWei(transaction.Value);

                if (!transactionReceipt.Succeeded(true))
                {
                    dappTransaction.DateUpdated = DateTime.Now;
                    dappTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    dappTransaction.BNBTransactionHash = transactionHash;

                    _unitOfWork.Commit();
                    _logger.LogError($"VerifyMetaMaskRequest: TransactionReceipt's status was failed: {transactionReceipt.Status.Value}");

                    return new GenericResult(false, "Your transction was invalid. Please Contact administrator for support!");
                }

                //send chat bot
                var depositMessage = TelegramBotHelper.BuildReportDepositPresaleMessage(
                                                    new Application.ViewModels.BotTelegram.DeFiMessageParam
                                                    {
                                                        Title = dappTransaction.Type.GetDescription(),
                                                        AmountBNB = balance,
                                                        DepositAt = DateTime.Now,
                                                        AmountToken = dappTransaction.TokenAmount,
                                                        UserWallet = dappTransaction.AddressFrom,
                                                        SystemWallet = dappTransaction.AddressTo,
                                                        Email = dappTransaction.Email
                                                    });

                await _botTelegramService.SendMessageAsyncWithSendingBalance(TelegramBotActionType.Deposit, depositMessage, TelegramBotHelper.DepositGroup);

                //compare dapp transaction with blockchain transaction
                if (!isRetry && dappTransaction.DAppTransactionState != DAppTransactionState.Requested)
                {
                    dappTransaction.DateUpdated = DateTime.Now;
                    dappTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    dappTransaction.BNBTransactionHash = transactionHash;

                    _unitOfWork.Commit();
                    _logger.LogError($"VerifyMetaMaskRequest: MetaMaskState was not matched: {dappTransaction.DAppTransactionState}");

                    return new GenericResult(false, "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare transaction with blockchain transaction
                if (dappTransaction.AddressFrom.ToLower() != transaction.From.ToLower() || dappTransaction.AddressTo.ToLower() != transaction.To.ToLower())
                {
                    dappTransaction.DateUpdated = DateTime.Now;
                    dappTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    dappTransaction.BNBTransactionHash = transactionHash;

                    _unitOfWork.Commit();
                    _logger.LogError($"VerifyMetaMaskRequest: Transaction's infor was not matched: ", transaction);

                    return new GenericResult(false, "Your transction was invalid. Please Contact administrator for support!");
                }

                //compare amount
                if (balance != dappTransaction.BNBAmount)
                {
                    dappTransaction.DateUpdated = DateTime.Now;
                    dappTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    dappTransaction.BNBTransactionHash = transactionHash;

                    _unitOfWork.Commit();
                    _logger.LogError($"VerifyMetaMaskRequest: Transaction's balance was not matched: {balance}");

                    return new GenericResult(false, "Your transction was invalid. Please Contact administrator for support!");
                }

                dappTransaction.DAppTransactionState = DAppTransactionState.Confirmed;
                dappTransaction.BNBTransactionHash = transactionHash;
                dappTransaction.DateUpdated = DateTime.Now;
                _unitOfWork.Commit();

                var tokentTranansaction = await _blockChain
                           .SendERC20Async(CommonConstants.BEP20EXCHANGEPrKey,
                           dappTransaction.AddressFrom,
                           CommonConstants.BEP20TokenContract, dappTransaction.TokenAmount,
                           CommonConstants.BEP20TokenDP, CommonConstants.BEP20Url);

                if (tokentTranansaction.Succeeded(true))
                {
                    dappTransaction.TokenAmount = dappTransaction.TokenAmount;
                    dappTransaction.TokenTransactionHash = tokentTranansaction.TransactionHash;
                    _unitOfWork.Commit();

                    var receivedMessage = TelegramBotHelper.BuildReportReceivePresaleMessage(new Application.ViewModels.BotTelegram.DeFiMessageParam
                    {
                        AmountBNB = balance,
                        ReceivedAt = DateTime.Now,
                        AmountToken = dappTransaction.TokenAmount,
                        UserWallet = dappTransaction.AddressFrom,
                        SystemWallet = CommonConstants.BEP20EXCHANGEPuKey,
                        Currency = "MAR",
                        Title = dappTransaction.Type.GetDescription(),
                        Email = dappTransaction.Email
                    });

                    await _botTelegramService.SendMessageAsyncWithSendingBalance(TelegramBotActionType.Deposit, receivedMessage, TelegramBotHelper.WithdrawGroup);

                    //process commission
                    if (dappTransaction.Type == DAppTransactionType.Claim)
                    {
                        if (!dappTransaction.Email.IsMissing())
                        {
                            var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.IsSystem);
                            if (admin == null)
                            {
                                _logger.LogInformation("Admin Is Not Found");
                            }
                            var ClaimUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == dappTransaction.Email);
                            if (ClaimUser == null)
                            {
                                _logger.LogInformation("ClaimUser Is Not Found : {0}", dappTransaction.AppUserId.GetValueOrDefault());
                            }
                            if (ClaimUser.ReferralId != admin.Id)
                            {
                                await _queueRepository.AddAsync(new QueueTask
                                {
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = dappTransaction.AddressFrom,
                                    Job = "Commission",
                                    Setting = JsonSerializer.Serialize(new CommissionSetting
                                    {
                                        Amount = dappTransaction.TokenAmount * 0.2m,
                                        UserReferral = ClaimUser.ReferralId.GetValueOrDefault(),
                                        UserAdmin = admin.Id,
                                        Email = dappTransaction.Email
                                    })
                                });
                                _unitOfWork.Commit();
                            }
                        }
                    }
                }
                _logger.LogInformation($"End call VerifyMetaMaskRequest with transaction hash: {tokentTranansaction.TransactionHash}");

                return new GenericResult(true, "Successed to Buy MAR");
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var metamaskTransaction = _dappRepository.FindById(Guid.Parse(tempDappTransaction));

                    metamaskTransaction.BNBTransactionHash = transactionHash;
                    metamaskTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return new GenericResult(false, e.Message);
            }
            
        }
        public async Task ProcessCommission(decimal amount,Guid userRef,Guid userAdmin, int level = 1)
        {
            try
            {
                if (userRef == userAdmin) {
                    return;
                }
                _logger.LogInformation("Start ProcessCommission amount {0},  referralAddress {1}, level {2}", amount, userRef, level);
                if (level > 5)
                {
                    return;
                }
                var referralUser = await _userManager.FindByIdAsync(userRef.ToString());
                if (referralUser == null)
                {
                    return;
                }
                decimal affiliateAmount = 0;

                switch (level)
                {
                    case 1:
                        affiliateAmount = amount * 0.4m;
                        break;
                    case 2:
                        affiliateAmount = amount * 0.2m;
                        break;
                    case 3:
                        affiliateAmount = amount * 0.2m;
                        break;
                    case 4:
                        affiliateAmount = amount * 0.1m;
                        break;
                    case 5:
                        affiliateAmount = amount * 0.1m;
                        break;
                    default:
                        break;
                }

                try
                {

                    var hasClaimed = await HasClaim(referralUser.Email);

                    if (hasClaimed)
                    {
                        referralUser.MARBalance += affiliateAmount;

                        await _marTransactinoRepository.AddAsync(new WalletMARTransaction
                        {
                            AppUserId = referralUser.Id,
                            AddressFrom = "SYSTEM",
                            AddressTo = "Wallet MAR",
                            Amount = affiliateAmount,
                            AmountReceive = affiliateAmount,
                            Type = WalletMARTransactionType.AffiliateClaim,
                            DateCreated = DateTime.Now,
                            FeeAmount = 0,
                            Fee = 0,
                            TransactionHash = "SYSTEM",
                        });

                        _unitOfWork.Commit();
                    }

                    //var tokenTransactionHash = await _blockChain
                    //           .SendERC20Async(CommonConstants.BEP20EXCHANGEPrKey,
                    //           referralUser.BNBBEP20PublishKey,
                    //           CommonConstants.BEP20TokenContract, affiliateAmount,
                    //           CommonConstants.BEP20TokenDP, CommonConstants.BEP20Url);


                    //if (tokenTransactionHash.Succeeded(true))
                    //{
                    //    await _tokenAffiliateTransactinoRepository.AddAsync(new WalletTokenAffiliateTransaction
                    //    {
                    //        AppUserId = referralUser.Id,
                    //        AddressFrom = "SYSTEM",
                    //        AddressTo = referralUser.BNBBEP20PublishKey,
                    //        Amount = affiliateAmount,
                    //        AmountReceive = affiliateAmount,
                    //        Type = WalletTokenAffiliateTransactionType.AffiliateClaim,
                    //        DateCreated = DateTime.Now,
                    //        FeeAmount = 0,
                    //        Fee = 0,
                    //        TransactionHash = tokenTransactionHash.TransactionHash,
                    //    });

                    //    _unitOfWork.Commit();
                    //}
                    //else
                    //{
                    //    _logger.LogInformation($"Failed to send token, referralAddress:  {referralUser.BNBBEP20PublishKey} - affiliateAmount: {affiliateAmount}");
                    //}
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"Failed to send token, referralAddress:  {referralUser.BNBBEP20PublishKey} - affiliateAmount: {affiliateAmount}");
                    _logger.LogInformation("Internal Server: {@0}", e);
                }

                level++;              
                if (!referralUser.BNBBEP20PublishKey.IsMissing())
                {
                    await ProcessCommission(amount, referralUser.ReferralId.GetValueOrDefault(), userAdmin, level);
                }

                _logger.LogInformation("End ProcessCommission");
            }
            catch (Exception e)
            {
                _logger.LogInformation("ProcessCommission referralAddress {0}, Error: {@1}", userRef, e);
            }
        }
        public async Task<PagedResult<DAppTransaction>> GetTransactionsAsync(string key, int pageIndex, int pageSize, string type, string userId)
        {
            var dappType = type.ParseEnum<DAppTransactionType>();

            var query = _dappRepository.FindAll(x => x.DAppTransactionState == DAppTransactionState.Confirmed && x.AppUserId == Guid.Parse(userId) && x.Type == dappType);

            if (!key.IsMissing())
            {
                query = query.Where(x => x.AddressFrom.Contains(key) || x.AddressTo.Contains(key) || x.BNBTransactionHash.Contains(key) || x.TokenTransactionHash.Contains(key));
            }

            var totalRow = query.Count();

            var transactions = await query.Skip((pageIndex - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            return new PagedResult<DAppTransaction>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = transactions,
                RowCount = totalRow
            };
        }

        public async Task<PagedResult<DAppTransactionView>> GetTransactionsAsync(string keyword, int pageIndex, int pageSize)
        {
           
            var query = _dappRepository.FindAll();

            if (!keyword.IsMissing())
            {
                query = query.Where(x => x.AddressFrom.Contains(keyword) 
                || x.AddressTo.Contains(keyword) 
                || x.BNBTransactionHash.Contains(keyword)
                || x.Email.Contains(keyword)
                || x.TokenTransactionHash.Contains(keyword));
            }

            var totalRow = query.Count();

            var transactions = await query.Skip((pageIndex - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            var data = transactions.Select(x => new DAppTransactionView
            {
                Id = x.Id,
                AddressTo = x.AddressTo,
                AddressFrom = x.AddressFrom,
                AppUserId = x.AppUserId,
                Email = x.Email,
                DAppTransactionState = x.DAppTransactionState,
                DAppTransactionStateName = x.DAppTransactionState.GetDescription(),
                Type = x.Type,
                TypeName = x.Type.GetDescription(),
                BNBAmount = x.BNBAmount,
                TokenAmount = x.TokenAmount,
                DateCreated = x.DateCreated,
                DateUpdated = x.DateUpdated,
                BNBTransactionHash = x.BNBTransactionHash,
                TokenTransactionHash = x.TokenTransactionHash,
                IsDevice = x.IsDevice,
                WalletType = x.WalletType,
            }).ToList();

            return new PagedResult<DAppTransactionView>()
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                Results = data,
                RowCount = totalRow
            };
        }

        private async Task<GenericResult> ValidateClaimTokenParams(DappInitializationParams model)
        {
            if (model.Email.IsMissing())
            {
                return GenericResult.ToFail("Please, Enter your registered email!!");
            }

            var hasUser = await _userManager.Users.AnyAsync(u => u.Email == model.Email);

            if (!hasUser)
            {
                return GenericResult.ToFail("Please, register an account before claim!!");
            }

            var hasAddress = await _dappRepository.FindAll(x => x.Type == DAppTransactionType.Claim
                                                                && (x.Email == model.Email)
                                                                && x.DAppTransactionState == DAppTransactionState.Confirmed)
                                                  .AnyAsync();

            if (hasAddress)
            {
                return GenericResult.ToFail("You have already claimed!!");
            }

            return GenericResult.ToSuccess();
        }

        private async Task<GenericResult> ValidateBuyTokenParams(DappInitializationParams model)
        {

            if (model.BNBAmount < 0.1m)
            {
                return new GenericResult(false, "Minimum buy 0.1 BNB");
            }

            if (model.BNBAmount > 6)
            {
                return new GenericResult(false, "Maximum buy 6 BNB");
            }

            var result = await _dappRepository.FindAll(x => x.AddressFrom == model.Address && x.DAppTransactionState == DAppTransactionState.Confirmed)
                                             .SumAsync(x => x.BNBAmount);

            //limit amount per wallet
            if ((result + model.BNBAmount) > 6)
            {
                return new GenericResult(false, "Maximum buy 6 BNB");
            }

            return new GenericResult(true);
        }

        public decimal ConculateTokenAmount(decimal bnbAmount, decimal priceBNBBep20)
        {
            decimal priceToken = 0.0004M;
            var amountUSD = Math.Floor(bnbAmount * priceBNBBep20);
            return Math.Floor(amountUSD / priceToken);
        }

        public  Task<bool> HasClaim(string email)
        {

            return _dappRepository.FindAll(x => x.Email == email && x.Type == DAppTransactionType.Claim && x.DAppTransactionState == DAppTransactionState.Confirmed).AnyAsync();
        }
    }
}
