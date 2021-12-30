using Core.Application.Filters;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Dapp;
using Core.Application.ViewModels.QueueTask;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Data.IRepositories;
using Core.Infrastructure.Interfaces;
using Core.Infrastructure.Telegram;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Core.Web.Models.RequestParams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Web.Api
{
    [Produces("application/json")]
    [Route("[Controller]/[action]")]
    [ApiController]
    
    public class DAPPController : Controller
    {
        private readonly TelegramBotWrapper _botTelegramService;
        private readonly IRepository<DAppTransaction, Guid> _dappTransaction;
        private readonly IRepository<QueueTask, int> _queueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBlockChainService _blockChain;
        private readonly ILogger<DAPPController> _logger;
        private readonly IDappService _dappService;
        private readonly UserManager<AppUser> _userManager;
        public const string DAppTransactionId = "DAppTransactionId";

        private static object _blockCreateUser = new object();
        private static Dictionary<string, DateTime> _requestedUsers;
        static Dictionary<string, DateTime> RequestedUsers
        {
            get
            {
                if (_requestedUsers == null)
                {
                    lock (_blockCreateUser)
                    {
                        _requestedUsers = new Dictionary<string, DateTime>();
                    }

                }

                return _requestedUsers;
            }
        }

        public DAPPController(TelegramBotWrapper botTelegramService,
                                  IRepository<DAppTransaction, Guid> metamaskTransaction,
                                  IUnitOfWork unitOfWork,
                                  IBlockChainService blockChain,
                                  ILogger<DAPPController> logger, IDappService dappService, UserManager<AppUser> userManager, IRepository<QueueTask, int> queueRepository)
        {
            _botTelegramService = botTelegramService;
            _dappTransaction = metamaskTransaction;
            _unitOfWork = unitOfWork;
            _blockChain = blockChain;
            _logger = logger;
            _dappService = dappService;
            _userManager = userManager;
            _queueRepository = queueRepository;
        }

        [HttpPost("{type}")]
        public async Task<IActionResult> InitializeTransactionProgress([FromBody] DappInitializationParams model, string type)
        {
            try
            {
                model.AppUserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("Start call InitializeTransactionProgress with param: {@0}", model);

                var res = await _dappService.InitializeTransactionProgress(model, type);

                if (!res.result.Success)
                {
                    return BadRequest(res.result);
                }

                HttpContext.Session.Set<string>(DAppTransactionId, res.transactionId);

                _logger.LogInformation($"End call InitializeTransactionMetaMask");
                return Ok(res.result);
            }
            catch (Exception e)
            {
                _logger.LogError("InitializeTransactionProgress: {@0}", e);
                return BadRequest(new GenericResult(false, e.Message));
            }
        }

        /// <summary>
        /// VerifyTransactionRequest
        /// </summary>
        /// <param name="transactionHash"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> VerifyTransactionRequest([FromBody] string transactionHash)
        {
            try
            {
                var temptransactionId = HttpContext.Session.Get<string>(DAppTransactionId);
     
                await _queueRepository.AddAsync(new QueueTask
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Admin",
                    Job = "VerifyTransaction",
                    Setting = JsonSerializer.Serialize(new TransactionVerificationSetting
                    {
                        TempDAppTransactionId = temptransactionId,
                        TransactionHash = transactionHash,
                    }),
                    Status = QueueStatus.Pending
                });

                _unitOfWork.Commit();
                return Ok(new GenericResult(true, "We are processing your claim request, kindly wait few minutes and check your wallet."));
            }
            catch (Exception e)
            {
                _logger.LogError("Internal Error: {@0}", e);

                try
                {
                    var transactionId = HttpContext.Session.Get<string>(DAppTransactionId);
                    var metamaskTransaction = _dappTransaction.FindById(Guid.Parse(transactionId));

                    metamaskTransaction.BNBTransactionHash = transactionHash;
                    metamaskTransaction.DAppTransactionState = DAppTransactionState.Failed;
                    _unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Internal Error: {@0}", ex);
                }

                return BadRequest(new GenericResult(false, "Somethings went wrong! Please Contact administrator for support."));
            }
        }

        [HttpPost]
        public IActionResult UpdateErrorMetaMask(MetaMaskErrorParams model)
        {
            _logger.LogInformation("UpdateErrorMetaMask: {@0}", JsonSerializer.Serialize(model));
            var convertor = new Nethereum.Hex.HexConvertors.HexUTF8StringConvertor();
            var transactionId = convertor.ConvertFromHex(model.TransactionHex);
            _logger.LogInformation("UpdateErrorMetaMask: transactionId {@0}", transactionId);
            var result = _dappTransaction.FindById(Guid.Parse(transactionId));

            switch (model.ErrorCode)
            {
                case "4001":
                    result.DAppTransactionState = DAppTransactionState.Rejected;
                    break;

                case "-32603":
                    result.DAppTransactionState = DAppTransactionState.Failed;
                    break;

                default:
                    result.DAppTransactionState = DAppTransactionState.Failed;
                    break;
            }

            _unitOfWork.Commit();
            return Ok(new GenericResult(true, "Successed to update."));
        }

        [HttpGet("{amount}")]
        public async Task<IActionResult> GetAmountPerBNB(decimal amount)
        {
            try
            {
                decimal priceBNBBep20 = _blockChain.GetCurrentPrice("BNB", "USD");
                if (priceBNBBep20 == 0)
                    return new OkObjectResult(new GenericResult(false, "There is a problem loading the currency value!"));

            }
            catch (Exception e)
            {
                return BadRequest(new GenericResult(false, "Invalid amount"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DAppConnect([FromQuery] string address, [FromQuery] string referral)
        {
            try
            {
                return BadRequest(new GenericResult(false, "Something went wrong!"));
                AddressUtil addressUtil = new AddressUtil();
                var convertedAddress = addressUtil.ConvertToChecksumAddress(address);

                if (!addressUtil.IsChecksumAddress(convertedAddress) || !addressUtil.IsValidAddressLength(convertedAddress))
                {
                    return new OkObjectResult(new GenericResult(false, "Address not in standard format BEP20."));
                }

                var convertedAddressRef = addressUtil.ConvertToChecksumAddress(referral);
                if (!addressUtil.IsChecksumAddress(convertedAddressRef) || !addressUtil.IsValidAddressLength(convertedAddressRef))
                {
                    return Ok(GenericResult.ToSuccess("", new { Address = address, Referral = "" }));
                }
                HttpContext.Session.Set<string>(AntiForgeryAddress.ConnectedAddress, address);
                var user = await _userManager.FindByNameAsync(address);

                if (user != null)
                {

                    lock (_blockCreateUser)
                    {
                        if (RequestedUsers.ContainsKey(address))
                            RequestedUsers.Remove(address);
                    }

                    return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = "" }));
                }

                if (referral.Trim().ToLower() == address.Trim().ToLower())
                {
                    var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.IsSystem);
                    referral = admin.UserName;
                }

                var appUser = new AppUser
                {
                    UserName = address,
                    DateCreated = DateTime.Now,
                    IsSystem = false
                };

                var result = await _userManager.CreateAsync(appUser);

                lock (_blockCreateUser)
                {
                    if (RequestedUsers.ContainsKey(address))
                        RequestedUsers.Remove(address);
                }

                if (!result.Succeeded)
                {
                    return BadRequest(new GenericResult(false, "Something went wrong!"));
                }

                return Ok(GenericResult.ToSuccess("successed to connect metamask!", new { Address = address, Referral = "" }));

                return Ok(GenericResult.ToSuccess());
            }
            catch (Exception e)
            {
                lock (_blockCreateUser)
                {
                    if (RequestedUsers.ContainsKey(address))
                        RequestedUsers.Remove(address);
                }
                return BadRequest(new GenericResult(false, "Something went wrong!"));
            }
        }

        [HttpGet]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> GetTransactions([FromQuery] string key, string type, [FromQuery] int page, [FromQuery] int pageSize)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

                var transactions = await _dappService.GetTransactionsAsync(key, page, pageSize, type, userId);

                return Ok(transactions);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
