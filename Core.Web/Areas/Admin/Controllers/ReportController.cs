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
using Core.Application.ViewModels.Report;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Core.Areas.Admin.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IReportService _reportService;

        public ReportController(
            IReportService reportService,
            UserManager<AppUser> userManager,
            IUserService userService
            )
        {
            _reportService = reportService;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() != "admin")
                return Redirect("/login");

            return View();
        }

        [HttpGet]
        public IActionResult GetReportInfo(string fromDate, string toDate)
        {
            var model = _reportService.GetReportInfo(fromDate, toDate);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(int refIndex, string keyword, int page, int pageSize)
        {
            var userId = User.GetSpecificClaim("UserId");
            var model = _userService.GetCustomerReferralPagingAsync(userId, refIndex, keyword, page, pageSize);
            return new OkObjectResult(model);
        }
    }
}
