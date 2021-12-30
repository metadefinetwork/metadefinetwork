using Core.Application.Interfaces;
using Core.Areas.Admin.Controllers;
using Core.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Core.Web.Areas.Admin.Controllers
{
    public class DaapTransactionController : BaseController
    {
        private readonly IDappService _dappService;

        public DaapTransactionController(IDappService dappService)
        {
            _dappService = dappService;
        }

        public IActionResult Index()
        {
            //if (!IsAdmin)
            //{
            //    return Redirect("/dashboard");
            //}
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaging(string keyword, int page, int pageSize)
        {
            try
            {
                var transactions = await _dappService.GetTransactionsAsync(keyword, page, pageSize);

                return Ok(transactions);
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }
    }
}
