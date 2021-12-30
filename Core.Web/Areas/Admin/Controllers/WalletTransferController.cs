using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BlockChain;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    public class WalletTransferController : BaseController
    {
        public readonly IWalletTransferService _walletTransferService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ITRONService _tronService;
        private readonly IBlockChainService _blockChainService;

        public WalletTransferController(
            IBlockChainService blockChainService,
            UserManager<AppUser> userManager,
            ITRONService tronService,
            IWalletTransferService walletTransferService
            )
        {
            _blockChainService = blockChainService;
            _userManager = userManager;
            _tronService = tronService;
            _walletTransferService = walletTransferService;
        }

        public IActionResult Index()
        {
            ViewBag.TotalTransactionAmount = _walletTransferService.GetTotalTransactionAmount();

            return View();
        }

        public class TransferModel
        {
            public int TotalWallet { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoTransfer(string modelJson)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<TransferModel>(modelJson);

                for (int i = 1; i <= model.TotalWallet; i++)
                {
                    var accountBep20 = _blockChainService.CreateERC20AndBEP20Account();

                    var transactionReceiptBEP20 = await _blockChainService.SendERC20Async(
                        CommonConstants.BEP20EXCHANGEPrKey, accountBep20.Address,
                        CommonConstants.BEP20TokenContract, model.Amount,
                        CommonConstants.BEP20TokenDP, CommonConstants.BEP20Url);

                    if (transactionReceiptBEP20.Status.Value==1)
                    {
                        var walletTransfer = new WalletTransferViewModel
                        {
                            PrivateKey = accountBep20.PrivateKey,
                            PublishKey = accountBep20.Address,
                            DateCreated = DateTime.Now,
                            TransactionHash = transactionReceiptBEP20.TransactionHash,
                            Amount = model.Amount
                        };

                        _walletTransferService.Add(walletTransfer);
                        _walletTransferService.Save();
                    }
                }

                return new OkObjectResult(new GenericResult(true, $"Auto Transfer is success."));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _walletTransferService.GetAllPaging(keyword, page, pageSize);
            return new OkObjectResult(model);
        }
    }
}
