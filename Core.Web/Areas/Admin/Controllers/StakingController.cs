using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Staking;
using Core.Application.ViewModels.System;
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
    public class StakingController : BaseController
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
        private readonly IStakingService _stakingService;
        private readonly IStakingRewardService _stakingRewardService;

        public StakingController(
            IStakingRewardService stakingRewardService,
            IStakingService stakingService,
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
            _stakingRewardService = stakingRewardService;
            _stakingService = stakingService;
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
            //var enumStakingTimeLines = ((StakingTimeLine[])Enum.GetValues(typeof(StakingTimeLine)))
            //    .Select(c => new EnumModel()
            //    {
            //        Value = (int)c,
            //        Name = c.GetDescription()
            //    }).ToList();

            //ViewBag.TimeLineType = new SelectList(enumStakingTimeLines, "Value", "Name");

            //var enumStakingPackages = ((StakingPackage[])Enum.GetValues(typeof(StakingPackage)))
            //    .Select(c => new EnumModel()
            //    {
            //        Value = (int)c,
            //        Name = c.GetDescription()
            //    }).ToList();

            //ViewBag.PackageType = new SelectList(enumStakingPackages, "Value", "Name");

            return View();
        }

        [HttpGet]
        public IActionResult GetPackageAllPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
                appUserId = User.GetSpecificClaim("UserId");

            var model = _stakingService.GetAllPaging(keyword, appUserId, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetCommissionAllPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
                appUserId = User.GetSpecificClaim("UserId");

            var model = _stakingRewardService.GetAllPaging(keyword, appUserId, page, pageSize);

            return new OkObjectResult(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyStaking(string modelJson)
        {
            return Ok();
        }

        [HttpGet]
        public IActionResult CaculationStakingByType(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<BuyStakingViewModel>(modelJson);
                var amount = (int)model.Package;
                if (model.Type == 3)
                {
                    model.AmountPayment = amount;
                    return new OkObjectResult(model);
                }
                else
                {
                    decimal price = 0.12M;
                    decimal paymentUSD = price * amount;

                    decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                    if (priceBNBBep20 == 0)
                        return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                    var paymentBNB = Math.Round(paymentUSD / priceBNBBep20, 4);

                    model.AmountPayment = paymentBNB;

                    return new OkObjectResult(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GameController_CaculationStakingByType: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlanceByType(int type)
        {
            var userId = User.GetSpecificClaim("UserId");
            var appUser = await _userService.GetById(userId);
            decimal walletBalance = 0;
            if (type == 1)
                walletBalance = appUser.BNBBalance;
            else
                walletBalance = appUser.MARBalance;

            return new OkObjectResult(walletBalance);
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlance()
        {
            var userId = User.GetSpecificClaim("UserId");

            var appUser = await _userService.GetById(userId);

            var model = new WalletViewModel()
            {
                BNBBalance = appUser.BNBBalance,
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
