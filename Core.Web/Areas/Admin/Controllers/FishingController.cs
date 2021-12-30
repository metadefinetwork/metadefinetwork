using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.BotTelegram;
using Core.Application.ViewModels.Fishing;
using Core.Application.ViewModels.Staking;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Infrastructure.Interfaces;
using Core.Services;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class FishingController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<ExchangeController> _logger;
        private readonly AddressUtil _addressUtil = new AddressUtil();
        private readonly IEmailSender _emailSender;
        private readonly IChartRoundService _chartRoundService;
        private readonly IStakingService _stakingService;
        private readonly IStakingRewardService _stakingRewardService;
        private readonly IRepository<StakingPeriod, int> _periodRepository;
        private readonly IRepository<ItemGame, Guid> _itemGameRepository;
        private readonly IRepository<Staking, int> _stakingRepository;
        private readonly IRepository<ItemGameUser, int> _itemGameUserRepository;
        private readonly IRepository<ItemGameUserLake, int> _itemGameUserLakeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public FishingController(
            IStakingRewardService stakingRewardService,
            IStakingService stakingService,
            IChartRoundService chartRoundService,
            ILogger<ExchangeController> logger,
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IUserService userService,
            IEmailSender emailSender,
            IBlockChainService blockChainService,
            IRepository<StakingPeriod, int> periodRepository,
            IRepository<ItemGame, Guid> itemGameRepository,
            IRepository<Staking, int> stakingRepository, 
            IRepository<ItemGameUser, int> itemGameUserRepository,
            IRepository<ItemGameUserLake, int> itemGameUserLakeRepository,
            IUnitOfWork unitOfWork)
        {
            _stakingRewardService = stakingRewardService;
            _stakingService = stakingService;
            _chartRoundService = chartRoundService;
            _logger = logger;
            _transactionService = transactionService;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _userService = userService;
            _emailSender = emailSender;
            _periodRepository = periodRepository;
            _itemGameRepository = itemGameRepository;
            _stakingRepository = stakingRepository;
            _itemGameUserRepository = itemGameUserRepository;
            _itemGameUserLakeRepository = itemGameUserLakeRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new FishingViewModel
            {
                Periods = await _periodRepository.FindAll().AsNoTracking().Select(x => new StakingPeriod { Interest = x.Interest, Id = x.Id, Period = x.Period }).OrderBy(x => x.Period).ToListAsync(),
                Lake = await _itemGameUserLakeRepository.FindAll().AsNoTracking().Select(x => new MyLakeViewModel { 
                    Amount = x.Amount,
                    DateCreated = x.DateCreated,
                    Id = x.Id,
                    ItemInfo = new ItemViewModel { 
                        Name = x.ItemGame.Name,
                        Id = x.ItemGame.Id,
                    }
                }).ToListAsync(),
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult GetItemAllPaging(string keyword,int type,int group,decimal price, int page, int pageSize)
        {
            var model = _stakingService.GetItemAllPaging(new ItemListingRequest { 
                KeyWord = keyword,
                Price = price,
                Type = type,
                Group = group,
                Page = page,
                PageSize = pageSize,
            });
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlanceByType(int type)
        {
            var userId = User.GetSpecificClaim("UserId");
            var appUser = await _userService.GetById(userId);
            decimal walletBalance = 0;
            if (type == 1)
            {
                walletBalance = appUser.MARBalance;
            }
            else if (type == 2)
            {
                walletBalance = appUser.MVRBalance;
            }
            else if (type == 3)
            {
                walletBalance = appUser.BNBBalance;
            }

            return new OkObjectResult(walletBalance);
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletBlance()
        {
            var userId = User.GetSpecificClaim("UserId");

            var appUser = await _userService.GetById(userId);

            var model = new WalletViewModel()
            {
                MARBalance = appUser.MARBalance,
                BNBBalance = appUser.BNBBalance,
            };

            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetMyItemAllPaging(int group, int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
                appUserId = User.GetSpecificClaim("UserId");

            if (string.IsNullOrEmpty(appUserId)) {
                return new OkObjectResult(new GenericResult(false, "user not found"));
            }

            var model = _stakingService.GetMyItemAllPaging(new MyItemListingRequest
            {
                AppUserId = appUserId,
                GroupId = group,
                Page = page,
                PageSize = pageSize,
            });
            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> BuyStaking(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<BuyItemGameViewModel>(modelJson);

                if(model == null)
                {
                    return BadRequest();
                }

                var userId = User.GetSpecificClaim("UserId");
                var appUser = await _userManager.FindByIdAsync(userId);

                var period = _periodRepository.FindById(model.PeriodId);
                var item = _itemGameRepository.FindById(Guid.Parse(model.ItemId));

                // Tính tiền chủ xị
                switch (model.WalletType)
                {
                    case 1:
                        if(appUser.MARBalance < item.Price)
                        {
                            return BadRequest();
                        }
                        appUser.MARBalance -= item.Price;

                        break;
                    case 2:
                        if (appUser.MVRBalance < item.Price)
                        {
                            return BadRequest();
                        }

                        appUser.MVRBalance -= item.Price;
                        break;
                    case 3:
                        decimal priceMAR = 0.0004M;
                        decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                        decimal paymentUSD = priceMAR * item.Price;

                        if (priceBNBBep20 == 0)
                            return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                        var paymentBNB = Math.Round(paymentUSD / priceBNBBep20, 4);

                        appUser.BNBBalance -= paymentBNB;

                        break;
                    default:
                        break;
                }

                await _userManager.UpdateAsync(appUser);

                var stacking = new Staking { DateCreated = DateTime.Now, LastReceived = DateTime.Now, Amount = item.Price, AppUserId = appUser.Id, InterestRate = period.Interest, Period = period.Period, Status = StakingType.Process, StakingPeriodId = period.Id, ItemGameId = item.Id };
                await _stakingRepository.AddAsync(stacking);

                _unitOfWork.Commit();
                return new OkObjectResult(new GenericResult(true, "Buy " + item.Name + " is successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_BuyStaking: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuyItemGame(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<BuyItemGameViewModel>(modelJson);

                if (model == null)
                {
                    return BadRequest();
                }

                var userId = User.GetSpecificClaim("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return new OkObjectResult(new GenericResult(false, "user not found"));
                }
                var appUser = await _userManager.FindByIdAsync(userId);
                var item = _itemGameRepository.FindById(Guid.Parse(model.ItemId));

                if (item == null)
                {
                    return new OkObjectResult(new GenericResult(false, "Item not found !"));
                }
                var itemUser = _itemGameUserRepository.FindSingle(x => x.AppUserId == Guid.Parse(userId) && x.ItemGameId == Guid.Parse(model.ItemId));
                if (itemUser != null && itemUser.ItemType == ItemType.TANK)
                {
                    return new OkObjectResult(new GenericResult(false, "You already own a " + item.Name));
                }
                switch (model.WalletType)
                {
                    case 1:
                        if (appUser.MARBalance < item.Price)
                        {
                            return BadRequest();
                        }
                        appUser.MARBalance -= item.Price;

                        break;
                    case 2:
                        if (appUser.MVRBalance < item.Price)
                        {
                            return BadRequest();
                        }

                        appUser.MVRBalance -= item.Price;
                        break;
                    case 3:
                        decimal priceMAR = 0.0004M;
                        decimal priceBNBBep20 = _blockChainService.GetCurrentPrice("BNB", "USD");
                        decimal paymentUSD = priceMAR * item.Price;

                        if (priceBNBBep20 == 0)
                            return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

                        var paymentBNB = Math.Round(paymentUSD / priceBNBBep20, 4);

                        appUser.BNBBalance -= paymentBNB;

                        break;
                    default:
                        break;
                }

                await _userManager.UpdateAsync(appUser);

                var itemGameUser = new ItemGameUser { DateCreated = DateTime.Now, Amount = item.Price, AppUserId = appUser.Id, ItemGameId = item.Id, ItemType = item.Type, Status = Status.Active, Type = ItemTypeUser.Buy };
                await _itemGameUserRepository.AddAsync(itemGameUser);
                if (item.Type == ItemType.TANK) {
                    var itemGameLake = new ItemGameUserLake { DateCreated = DateTime.Now, Amount = item.Price, AppUserId = appUser.Id, ItemGameId = item.Id, Status = StatusLake.Active };
                    await _itemGameUserLakeRepository.AddAsync(itemGameLake);
                }
                _unitOfWork.Commit();
                return new OkObjectResult(new GenericResult(true, "Buy "+ item.Name + " is successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_BuyStaking: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult GetHistoryStacking(int page, int pageSize)
        {
            string appUserId = string.Empty;

            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "customer")
                appUserId = User.GetSpecificClaim("UserId");

            var model = _stakingService.GetHistoryRewardAllPaging(new HistoryRewardListingRequest { Page = page,PageSize=pageSize,AppUserId = appUserId});

            return new OkObjectResult(model);
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

        [HttpGet]
        public IActionResult GetAffiliateAllPaging(string keyword, int page, int pageSize)
        {
            //string appUserId = string.Empty;

            //var roleName = User.GetSpecificClaim("RoleName");
            //if (roleName.ToLower() == "customer")
            //    appUserId = User.GetSpecificClaim("UserId");

            //var model = _stakingAffiliateService.GetAllPaging(keyword, appUserId, page, pageSize);

            return new OkObjectResult(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCommission(int id)
        {
            try
            {
                var userId = User.GetSpecificClaim("UserId");

                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                    return new OkObjectResult(new GenericResult(false, "Account does not exist"));

                var staking = _stakingService.GetById(id);
                if (staking == null)
                    return new OkObjectResult(new GenericResult(false, "Package does not exist"));

                if (staking.Type == StakingType.Finish)
                    return new OkObjectResult(new GenericResult(false, "Package is finish"));

                if (staking.StakingTimes == staking.ReceiveTimes)
                    return new OkObjectResult(new GenericResult(false, "Package is finish"));

                if (((int)staking.TimeLine * 30) == staking.ReceiveTimes)
                    return new OkObjectResult(new GenericResult(false, "Package is finish"));

                if (staking.DateCreated.Date == DateTime.Now.Date)
                    return new OkObjectResult(new GenericResult(false, "Get interest starting tomorrow"));

                if (staking.ReceiveLatest.Date == DateTime.Now.Date)
                    return new OkObjectResult(new GenericResult(false, "You got today's interest"));

                var totalInterestRate = staking.InterestRate + appUser.StakingInterest;

                var timesAmount = Math.Round(staking.StakingAmount / staking.StakingTimes, 2);

                var commission = Math.Round(timesAmount * (totalInterestRate / 100), 2);

                var updateCommission = await _userManager.UpdateAsync(appUser);

                if (updateCommission.Succeeded)
                {
                    staking.ReceiveAmount += commission;
                    staking.ReceiveTimes += 1;
                    staking.ReceiveLatest = DateTime.Now;

                    if (staking.StakingTimes == staking.ReceiveTimes)
                        staking.Type = StakingType.Finish;

                    _stakingService.Update(staking);
                    _stakingService.Save();

                    var stakingReward = _stakingRewardService.Add(new StakingRewardViewModel
                    {
                        //AppUserId = appUser.Id,
                        //PackageInterestRate = staking.InterestRate,
                        //SuddenInterestRate = appUser.StakingInterest,
                        //RealInterestRate = totalInterestRate,
                        //DateCreated = DateTime.Now,
                        //Amount = commission,
                        //StakingId = staking.Id
                    });
                    _stakingRewardService.Save();

                    if (appUser.IsSystem == false && appUser.ReferralId.HasValue)
                    {
                        var referralF1 = await _userManager.FindByIdAsync(appUser.ReferralId.ToString());
                        if (referralF1 != null && referralF1.IsSystem == false && referralF1.StakingBalance >= 1000)
                        {
                            var referralF1Commission = Math.Round(commission * 0.2m, 2);

                            var updateReferralF1Affiliate = await _userManager.UpdateAsync(referralF1);
                            if (updateReferralF1Affiliate.Succeeded)
                            {
                                //_stakingAffiliateService.Add(new StakingAffiliateViewModel
                                //{
                                //    AppUserId = referralF1.Id,
                                //    DateCreated = DateTime.Now,
                                //    Amount = referralF1Commission,
                                //    StakingRewardId = stakingReward.Id
                                //});

                                //_stakingAffiliateService.Save();
                            }

                            if (referralF1.IsSystem == false && referralF1.ReferralId.HasValue)
                            {
                                var referralF2 = await _userManager.FindByIdAsync(referralF1.ReferralId.ToString());
                                if (referralF2 != null && referralF2.IsSystem == false && referralF2.StakingBalance >= 1000)
                                {
                                    var referralF2Commission = Math.Round(commission * 0.1m, 2);

                                    var updateReferralF2Affiliate = await _userManager.UpdateAsync(referralF2);
                                    if (updateReferralF2Affiliate.Succeeded)
                                    {
                                        //_stakingAffiliateService.Add(new StakingAffiliateViewModel
                                        //{
                                        //    AppUserId = referralF2.Id,
                                        //    DateCreated = DateTime.Now,
                                        //    Amount = referralF2Commission,
                                        //    StakingRewardId = stakingReward.Id
                                        //});

                                        //_stakingAffiliateService.Save();
                                    }

                                    if (referralF2.IsSystem == false && referralF2.ReferralId.HasValue)
                                    {
                                        var referralF3 = await _userManager.FindByIdAsync(referralF2.ReferralId.ToString());
                                        if (referralF3 != null && referralF3.IsSystem == false && referralF3.StakingBalance >= 1000)
                                        {
                                            var referralF3Commission = Math.Round(commission * 0.05m, 2);

                                            var updateReferralF3Affiliate = await _userManager.UpdateAsync(referralF3);
                                            if (updateReferralF3Affiliate.Succeeded)
                                            {
                                                //_stakingAffiliateService.Add(new StakingAffiliateViewModel
                                                //{
                                                //    AppUserId = referralF3.Id,
                                                //    DateCreated = DateTime.Now,
                                                //    Amount = referralF3Commission,
                                                //    StakingRewardId = stakingReward.Id
                                                //});

                                                //_stakingAffiliateService.Save();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    return new OkObjectResult(new GenericResult(false, updateCommission.Errors.FirstOrDefault().Description));
                }

                return new OkObjectResult(new GenericResult(true, "Get interest is success"));
            }
            catch (Exception ex)
            {
                _logger.LogError("StakingController_GetCommission: {0}", ex.Message);
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
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
