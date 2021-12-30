using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Implementation;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Infrastructure.Telegram;
using Core.Services;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nethereum.Util;

namespace Core.Areas.Admin.Controllers
{
    public class WalletController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly ITransactionService _transactionService;
        private readonly ITRONService _tronService;
        private readonly ILogger<WalletController> _logger;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly IWalletBNBTransactionService _walletBNBTransactionService;
        private readonly IWalletMVRTransactionService _walletMVRTransactionService;
        private readonly ITicketTransactionService _ticketTransactionService;
        private readonly TelegramBotWrapper _botSrv;
        private readonly IEmailSender _emailSender;

        public WalletController(
            ITicketTransactionService ticketTransactionService,
            IWalletBNBTransactionService walletBNBTransactionService,
            IWalletMVRTransactionService walletMVRTransactionService,
            ILogger<WalletController> logger,
            ITRONService tronService,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IUserService userService,
            TelegramBotWrapper botSrv,
            IEmailSender emailSender,
            IBlockChainService blockChainService)
        {
            _ticketTransactionService = ticketTransactionService;
            _walletBNBTransactionService = walletBNBTransactionService;
            _walletMVRTransactionService = walletMVRTransactionService;
            _logger = logger;
            _tronService = tronService;
            _transactionService = transactionService;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _userService = userService;
            _botSrv = botSrv;
            _emailSender = emailSender;
        }

        [Route("wallet")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.GetSpecificClaim("UserId");
            var appUser = await _userService.GetById(userId);

            return View(new WalletViewModel
            {
                BEP20PublishKey = appUser.BEP20PublishKey,
                BNBBEP20PublishKey = appUser.BNBBEP20PublishKey,
                AuthenticatorCode = appUser.AuthenticatorCode,
                Enabled2FA = appUser.Enabled2FA
            });
        }

        public class WithdrawMARViewModel
        {
            public decimal Amount { get; set; }
            public string AddressTo { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawWalletMAR([FromBody] WithdrawMARViewModel model, [FromQuery] string authenticatorCode)
        {
            try
            {
                var userId = User.GetSpecificClaim("UserId");

                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));

                var isMatched = await _userManager.CheckPasswordAsync(appUser, model.Password);

                if (!isMatched)
                    return new OkObjectResult(new GenericResult(false, "Wrong password"));

                if (appUser.TwoFactorEnabled)
                {
                    var isValid = await VerifyCode(authenticatorCode, _userManager, appUser);

                    if (!isValid)
                        return new OkObjectResult(new GenericResult(false, "Invalid authenticator code"));
                }

                if (model.Amount < 10000)
                    return new OkObjectResult(new GenericResult(false, "Minimum withdraw 10,000 MAR"));

                if (model.Amount > appUser.MARBalance)
                    return new OkObjectResult(new GenericResult(false, "Your balance is not enough to make a transaction"));

                if (appUser.BNBBEP20PublishKey.IsAnEmptyAddress())
                    return new OkObjectResult(new GenericResult(false, "Please, update the wallet address in your profile."));

                appUser.MARBalance -= model.Amount;
                var resultMARBalanceUpdate = await _userManager.UpdateAsync(appUser);

                if (resultMARBalanceUpdate.Succeeded)
                {
                    decimal fee = 0.02m;
                    decimal feeAmount = model.Amount * fee;
                    decimal amountReceive = model.Amount - feeAmount;

                    var ticketTransaction = new TicketTransactionViewModel
                    {
                        Fee = fee,
                        FeeAmount = feeAmount,
                        AmountReceive = amountReceive,
                        Amount = model.Amount,
                        AddressTo = appUser.BNBBEP20PublishKey,
                        AddressFrom = CommonConstants.BEP20EXCHANGEPuKey,
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now,
                        AppUserId = appUser.Id,
                        Unit = TicketTransactionUnit.MAR,
                        Status = TicketTransactionStatus.Pending,
                        Type = TicketTransactionType.WithdrawMAR
                    };
                    _ticketTransactionService.Add(ticketTransaction);
                    _ticketTransactionService.Save();

                    try
                    {
                        var parameters = new WithdrawMessageParam
                        {
                            Amount = amountReceive,
                            WithDrawTime = DateTime.Now,
                            Email = appUser.Email,
                            UserId = appUser.Sponsor,
                            Wallet = model.AddressTo,
                            Currency = TicketTransactionUnit.MAR.GetDescription(),
                            Rate = 0,
                            Fee = feeAmount,
                            Title = "Withdraw"
                        };

                        if (appUser.ReferralId.HasValue)
                        {
                            var refferal = await _userManager.FindByIdAsync(appUser.ReferralId.ToString());

                            parameters.SponsorEmail = refferal.Email;
                            parameters.SponsorId = refferal.Sponsor;
                        }

                        var message = TelegramBotHelper.BuildReportWITHDRAWMessage(parameters);

                        await _botSrv.SendMessageAsyncWithSendingBalance(TelegramBotActionType.Withdraw, message, TelegramBotHelper.WithdrawGroup);
                    }
                    catch (Exception ex)
                    {
                    }

                    return new OkObjectResult(new GenericResult(true,
                        "Create request withdraw from Wallet MAR successful"));
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false,
                        resultMARBalanceUpdate.Errors.FirstOrDefault().Description));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("WalletController_WithdrawWalletMAR: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult GetAllTicketTransactionPaging(string keyword, int page, int pageSize)
        {
            string appUserId = User.GetSpecificClaim("UserId");

            var model = _ticketTransactionService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlance()
        {
            try
            {
                var userId = User.GetSpecificClaim("UserId");

                var appUser = await _userManager.FindByIdAsync(userId);

                var model = new WalletViewModel()
                {
                    MARBalance = appUser.MARBalance,
                    MVRBalance = appUser.MVRBalance,
                    BNBBalance = appUser.BNBBalance,
                    StakingBalance = appUser.StakingBalance,
                    StakingInterest = appUser.StakingInterest
                };

                return new OkObjectResult(new GenericResult(true, model));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }
    }
}
