using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.System;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Services;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class ExchangeController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IWalletMVRTransactionService _walletMVRTransactionService;
        private readonly IWalletBNBTransactionService _walletBNBTransactionService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<ExchangeController> _logger;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly IEmailSender _emailSender;
        private readonly IChartRoundService _chartRoundService;

        public ExchangeController(
            IWalletBNBTransactionService walletBNBTransactionService,
            IChartRoundService chartRoundService,
            IWalletMVRTransactionService walletMVRTransactionService,
            ILogger<ExchangeController> logger,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IUserService userService,
            IEmailSender emailSender,
            IBlockChainService blockChainService)
        {
            _walletBNBTransactionService = walletBNBTransactionService;
            _chartRoundService = chartRoundService;
            _walletMVRTransactionService = walletMVRTransactionService;
            _logger = logger;
            _transactionService = transactionService;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _userService = userService;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        public class BuyICDViewModel
        {
            public int Type { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> By(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<>(modelJson);

                var userId = User.GetSpecificClaim("UserId");

                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));

                if (model.Amount < 0.1m)
                    return new OkObjectResult(new GenericResult(false, "Minimum buy 0.1 BNB"));

                if (model.Amount > 6)
                    return new OkObjectResult(new GenericResult(false, "Maximum buy 6 BNB"));

                decimal payPrivateSale = _transactionService.GetUserAmountByType(appUser.Id, TransactionType.PayPreSale);
                if ((payPrivateSale + model.Amount) > 6)
                    return new OkObjectResult(new GenericResult(false, "Maximum buy 6 BNB"));

                if (model.Type == 1)
                {
                    if (model.Amount > appUser.BNBBalance)
                        return new OkObjectResult(new GenericResult(false, "Your balance is not enough to make a transaction"));
                }
                else
                    return new OkObjectResult(new GenericResult(false, "Wallet type is required"));

                decimal price = 0.2M;

                decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                if (priceBNBBep20 == 0)
                    return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                var amountUSD = Math.Round(model.Amount * priceBNBBep20, 2);

                decimal amount = Math.Round(amountUSD / price, 2);

                appUser.MARBalance += amount;
                appUser.BNBBalance -= model.Amount;

                var resultAppUserUpdate = await _userManager.UpdateAsync(appUser);

                if (resultAppUserUpdate.Succeeded)
                {
                    _walletBNBTransactionService.Add(
                        new WalletBNBTransactionViewModel
                        {
                            AddressFrom = "Wallet BNB",
                            AddressTo = "SYSTEM",
                            Amount = model.Amount,
                            AmountReceive = model.Amount,
                            Fee = 0,
                            FeeAmount = 0,
                            DateCreated = DateTime.Now,
                            AppUserId = appUser.Id,
                            TransactionHash = "SYSTEM",
                            Type = WalletBNBTransactionType.PayPreSale
                        });
                    _walletBNBTransactionService.Save();


                    #region Add MVR Transaction

                    _walletMVRTransactionService.Add(new WalletMVRTransactionViewModel
                    {
                        AddressFrom = "SYSTEM",
                        AddressTo = "Wallet MVR",
                        Amount = amount,
                        AmountReceive = amount,
                        Fee = 0,
                        FeeAmount = 0,
                        DateCreated = DateTime.Now,
                        AppUserId = appUser.Id,
                        TransactionHash = "SYSTEM",
                        Type = WalletMVRTransactionType.DepositPreSale
                    });
                    _walletMVRTransactionService.Save();

                    #endregion Add MVR Transaction

                    #region Add Transaction

                    _transactionService.Add(new TransactionViewModel
                    {
                        TransactionHash = "SYSTEM",
                        Amount = amount,
                        AddressFrom = "SYSTEM",
                        AddressTo = "Wallet MVR",
                        DateCreated = DateTime.Now,
                        AppUserId = appUser.Id,
                    });
                    _transactionService.Save();

                    _transactionService.Add(new TransactionViewModel
                    {
                        TransactionHash = "SYSTEM",
                        Amount = model.Amount,
                        AddressFrom ="Wallet BNB",
                        AddressTo = "SYSTEM",
                        DateCreated = DateTime.Now,
                        AppUserId = appUser.Id,
                        Unit = TransactionUnit.BNB,
                        Type = TransactionType.PayPreSale,
                    });
                    _transactionService.Save();

                    #endregion Add Transaction

                    

                    return new OkObjectResult(new GenericResult(true, "Buy MAR is successful."));
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false,
                        resultAppUserUpdate.Errors.FirstOrDefault().Description));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("WalletController_BuyMAR: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult CaculationByBNB(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<>(modelJson);

                decimal price = 0.2M;

                decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                if (priceBNBBep20 == 0)
                    return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                var amountUSD = Math.Round(model.OrderBNB * priceBNBBep20, 2);

                decimal amount= Math.Round(amountUSD / price, 2);

                model.Amount = Math.Round(amount, 2);

                return new OkObjectResult(model);
            }
            catch (Exception ex)
            {
                _logger.LogError("ExchangeController_CaculationICDByBNB: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlanceByType(int type)
        {
            var userId = User.GetSpecificClaim("UserId");
            var appUser = await _userService.GetById(userId);
            decimal bnbBalance = 0;
            if (type == 1)
                bnbBalance = appUser.BNBBalance;

            return new OkObjectResult(bnbBalance);
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlance()
        {
            var userId = User.GetSpecificClaim("UserId");
            var appUser = await _userService.GetById(userId);
            var model = new WalletViewModel()
            {
                BNBBalance = appUser.BNBBalance
            };

            return new OkObjectResult(model);
        }

        private async Task SendComisionMail(TransactionViewModel vm)
        {
            try
            {
                var appUser = await _userManager.FindByIdAsync(vm.AppUserId.ToString());
                if (appUser != null)
                {
                    var parameters = new ComisionMessageParam
                    {
                        Amount = vm.Amount,
                        RecievedAt = DateTime.Now,
                        AddressTo = vm.AddressTo,
                        Unit = vm.Unit.ToString(),
                        Rate = vm.Unit == TransactionUnit.BNB ? vm.PriceBNB : 0,
                        AddressFrom = vm.AddressFrom
                    };

                    var message = _emailSender.BuildReportCommisionMessage(parameters);

                    await _emailSender.TrySendEmailAsync(appUser.Email, $"Commission received {DateTime.UtcNow.ToddMMyyyyHHmmss()}(UTC)", message);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to Send Commision Mail");
            }
        }
    }
}
