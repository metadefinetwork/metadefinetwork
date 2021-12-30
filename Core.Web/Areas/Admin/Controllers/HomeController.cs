using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Models;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Core.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : BaseController
    {
        private readonly IChartRoundService _chartRoundService;
        private readonly INotifyService _notifyService;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            IChartRoundService chartRoundService,
            UserManager<AppUser> userManager,
            INotifyService notifyService
            )
        {
            _chartRoundService = chartRoundService;
            _userManager = userManager;
            _notifyService = notifyService;
        }
        [Route("Dashboard")]
        public IActionResult Index([FromQuery] string referralLink)
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "admin")
                return Redirect("/admin/report/index");

            var model = new HomeViewModel();
            model.Notify = _notifyService.GetDashboard();
            //model.ChartRounds = _chartRoundService.GetAll();

            ViewData["referralLink"] = referralLink;
            return View(model);
        }

        [Route("Claim")]
        [AllowAnonymous]
        public IActionResult Claim()
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "admin")
                return Redirect("/admin/report/index");

            var model = new HomeViewModel();
            model.Notify = _notifyService.GetDashboard();
            model.ChartRounds = _chartRoundService.GetAll();

            //if (referralLink.IsMissing())
            //{
            //    var admin = _userManager.Users.FirstOrDefault(u => u.IsSystem); 
            //    referralLink = admin.UserName;
            //}

            //ViewData["referralLink"] = referralLink;
            return View(model);
        }

        [Route("Buy")]
        public IActionResult Buy([FromQuery] string referralLink)
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() == "admin")
                return Redirect("/admin/report/index");

            var model = new HomeViewModel();
            model.Notify = _notifyService.GetDashboard();
            model.ChartRounds = _chartRoundService.GetAll();

            ViewData["referralLink"] = referralLink;
            return View(model);
        }
    }
}
