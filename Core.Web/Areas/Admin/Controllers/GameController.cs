using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class GameController : BaseController
    {
        private readonly IWalletMVRTransactionService _walletMVRTransactionService;
        private readonly IWalletBNBTransactionService _walletBNBTransactionService;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly ITransactionService _transactionService;
        private readonly ITRONService _tronService;
        private readonly ILogger<GameController> _logger;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly ILuckyRoundHistoryService _luckyRoundHistoryService;
        private readonly ILuckyRoundService _luckyRoundService;
        private readonly IGameTicketService _gameTicketService;

        public GameController(
            IGameTicketService gameTicketService,
            IWalletMVRTransactionService walletMVRTransactionService,
            IWalletBNBTransactionService walletBNBTransactionService,
            ILuckyRoundHistoryService luckyRoundHistoryService,
            ILuckyRoundService luckyRoundService,
            ILogger<GameController> logger,
            ITRONService tronService,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IUserService userService,
            IBlockChainService blockChainService)
        {
            _gameTicketService = gameTicketService;
            _walletMVRTransactionService = walletMVRTransactionService;
            _walletBNBTransactionService = walletBNBTransactionService;
            _luckyRoundHistoryService = luckyRoundHistoryService;
            _luckyRoundService = luckyRoundService;
            _logger = logger;
            _tronService = tronService;
            _transactionService = transactionService;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        #region Ticket

        [HttpGet]
        public IActionResult GetAllTicketPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
                appUserId = User.GetSpecificClaim("UserId");

            var model = _gameTicketService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        #endregion Ticket

        #region LuckyRound

        [HttpGet]
        public IActionResult LuckyRound()
        {
            return View();
        }

        public List<SegmentViewModel> SetSegments(List<SegmentViewModel> Segments, int value, string message)
        {
            if (Segments == null)
                Segments = new List<SegmentViewModel>();

            var segment = Segments.FirstOrDefault(x => x.Value == value);
            if (segment == null)
                Segments.Add(new SegmentViewModel { Value = value, Message = message, Times = 1 });
            else
                segment.Times += 1;

            return Segments;
        }

        public class SegmentViewModel
        {
            public int Value { get; set; }
            public string Message { get; set; }
            public int Times { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetTicket()
        {
            string appUserId = User.GetSpecificClaim("UserId");
            var appUser = await _userManager.FindByIdAsync(appUserId);
            return new OkObjectResult(appUser.TicketBalance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyTicket(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<BuyTicketViewModel>(modelJson);
                var userId = User.GetSpecificClaim("UserId");

                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));

                if (model.TicketOrder < 5)
                    return new OkObjectResult(new GenericResult(false, "Minimum buy 5 TICKET"));

                if (model.Type == 1)
                {
                    decimal price = 0.1M;
                    decimal paymentUSD = price * (model.TicketOrder * 25);

                    decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                    if (priceBNBBep20 == 0)
                        return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                    var paymentBNB = Math.Round(paymentUSD / priceBNBBep20, 4);


                    if (paymentBNB > appUser.BNBBalance)
                        return new OkObjectResult(new GenericResult(false, "Your balance is not enough to make a transaction"));

                    appUser.TicketBalance += model.TicketOrder;
                    appUser.BNBBalance -= paymentBNB;

                    var resultMainBalanceUpdate = await _userManager.UpdateAsync(appUser);

                    if (resultMainBalanceUpdate.Succeeded)
                    {
                        #region Add Invest Transaction

                        _walletBNBTransactionService.Add(new WalletBNBTransactionViewModel
                        {
                            AddressFrom = "Wallet BNB",
                            AddressTo = "SYSTEM",
                            Amount = paymentBNB,
                            AmountReceive = paymentBNB,
                            Fee = 0,
                            FeeAmount = 0,
                            DateCreated = DateTime.Now,
                            AppUserId = appUser.Id,
                            TransactionHash = "SYSTEM",
                            Type = WalletBNBTransactionType.PayTicket
                        });
                        _walletBNBTransactionService.Save();

                        #endregion Add Invest Transaction

                        #region Add Game Ticket

                        _gameTicketService.Add(new GameTicketViewModel
                        {
                            AddressFrom = "Wallet BNB BEP20",
                            AddressTo = "Wallet Ticket",
                            Amount = model.TicketOrder,
                            DateCreated = DateTime.Now,
                            AppUserId = appUser.Id,
                            Type = GameTicketType.SwapFromWalletBNBBEP20
                        });
                        _gameTicketService.Save();

                        #endregion Add Game Ticket
                    }
                    else
                    {
                        return new OkObjectResult(new GenericResult(false,
                            resultMainBalanceUpdate.Errors.FirstOrDefault().Description));
                    }
                }
                else if (model.Type == 3)
                {
                    var payment = model.TicketOrder * 25;

                    if (payment > appUser.MARBalance)
                        return new OkObjectResult(new GenericResult(false, "Your balance is not enough to make a transaction"));

                    appUser.TicketBalance += model.TicketOrder;
                    appUser.MARBalance -= payment;

                    var resultInvestBalanceUpdate = await _userManager.UpdateAsync(appUser);

                    if (resultInvestBalanceUpdate.Succeeded)
                    {
                        #region Add MVR Transaction

                        _walletMVRTransactionService.Add(new WalletMVRTransactionViewModel
                        {
                            AddressFrom = "Wallet MVR",
                            AddressTo = "SYSTEM",
                            Amount = payment,
                            AmountReceive = payment,
                            Fee = 0,
                            FeeAmount = 0,
                            DateCreated = DateTime.Now,
                            AppUserId = appUser.Id,
                            TransactionHash = "SYSTEM",
                            Type = WalletMVRTransactionType.PayTicket
                        });
                        _walletMVRTransactionService.Save();

                        #endregion Add MVR Transaction

                        #region Add Game Ticket

                        _gameTicketService.Add(new GameTicketViewModel
                        {
                            AddressFrom = "Wallet MVR",
                            AddressTo = "Wallet Ticket",
                            Amount = model.TicketOrder,
                            DateCreated = DateTime.Now,
                            AppUserId = appUser.Id,
                            Type = GameTicketType.SwapFromWalletInvest
                        });
                        _gameTicketService.Save();

                        #endregion Add Game Ticket
                    }
                    else
                    {
                        return new OkObjectResult(new GenericResult(false,
                            resultInvestBalanceUpdate.Errors.FirstOrDefault().Description));
                    }
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false, "Wallet type is required"));
                }

                return new OkObjectResult(new GenericResult(true, "Buy Ticket is successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError("GameController_BuyTicket: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult CaculationTicketByType(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<BuyTicketViewModel>(modelJson);
                if (model.Type == 3)
                {
                    model.AmountPayment = model.TicketOrder * 25;
                    return new OkObjectResult(model);
                }
                else
                {
                    decimal price = 0.1M;
                    decimal paymentUSD = price * (model.TicketOrder * 25);

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
                _logger.LogError("ExchangeController_CaculationByBNB: {0}", ex.Message);
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
            {
                walletBalance = appUser.BNBBalance;
            }
            else
            {
                walletBalance = appUser.MARBalance;
            }

            return new OkObjectResult(walletBalance);
        }

        [HttpGet]
        public IActionResult GetAllLuckyRoundHistoryPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
            {
                appUserId = User.GetSpecificClaim("UserId");
            }

            var model = _luckyRoundHistoryService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        #endregion LuckyRound
    }
}
