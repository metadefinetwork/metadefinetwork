using System;
using System.Threading.Tasks;
using Core.Application.Filters;
using Core.Application.Interfaces;
using Core.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TransactionController : BaseController
    {
        public readonly ITransactionService _transactionService;
        public readonly IWalletBNBTransactionService _walletBNBTransactionService;
        public readonly IWalletMVRTransactionService _walletMVRTransactionService;
        public readonly IWalletMARTransactionService _walletMARTransactionService;
        private readonly UserManager<AppUser> _userManager;

        public TransactionController(
            IWalletMVRTransactionService walletMVRTransactionService,
            IWalletBNBTransactionService walletBNBTransactionService,
            UserManager<AppUser> userManager,
            ITransactionService transactionService,
            IWalletMARTransactionService walletMARTransactionService
            )
        {
            _walletMVRTransactionService = walletMVRTransactionService;
            _walletBNBTransactionService = walletBNBTransactionService;
            _walletMARTransactionService = walletMARTransactionService;
            _userManager = userManager;
            _transactionService = transactionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            if (IsCustomer)
                appUserId = CurrentUserId.ToString();

            var model = _transactionService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        [Route("bnb-transaction")]
        public IActionResult WalletBNB()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetWalletBNBPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            if (IsCustomer)
                appUserId = CurrentUserId.ToString();

            var model = _walletBNBTransactionService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        [Route("mvr-transaction")]
        public IActionResult WalletMVR()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetWalletMVRPaging(string keyword, int page, int pageSize)
        {
            string appUserId = string.Empty;

            if (IsCustomer)
                appUserId = CurrentUserId.ToString();

            var model = _walletMVRTransactionService.GetAllPaging(keyword, appUserId, page, pageSize);
            return new OkObjectResult(model);
        }

        [Route("mar-transaction")]
        public IActionResult WalletMAR()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetWalletMARPaging(string keyword, int page, int pageSize)
        {
            try
            {
                string appUserId = string.Empty;
                if (IsCustomer)
                    appUserId = CurrentUserId.ToString();

                var transactions = await _walletMARTransactionService.GetAllPagingSync(keyword, appUserId, page, pageSize);
                return new OkObjectResult(transactions);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
