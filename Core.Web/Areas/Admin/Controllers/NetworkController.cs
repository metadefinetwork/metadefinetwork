using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.Blog;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Product;
using Core.Data.Enums;
using Core.Extensions;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NetworkController : BaseController
    {
        private readonly IUserService _userService;

        public NetworkController(IUserService userService)
        {
            _userService = userService;
        }
        [Route("Network")]
        public async Task<IActionResult> Index()
        {
            var model = await _userService.GetNetworkInfo(CurrentUserId.ToString());
            return View(model);
        }
       
        public async Task<IActionResult> GetTotalNetworkInfo()
        {
            var model = await _userService.GetTotalNetworkInfo(CurrentUserId.ToString());
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(int refIndex, string keyword, int page, int pageSize)
        {
            var model = _userService.GetCustomerReferralPagingAsync(CurrentUserId.ToString(), refIndex, keyword, page, pageSize);
            return new OkObjectResult(model);
        }
        
        public async Task<IActionResult> GetNetworkInfo()
        {
            var model = await _userService.GetNetworkInfo(CurrentUserId.ToString());
            return new OkObjectResult(model);
        }
    }
}


