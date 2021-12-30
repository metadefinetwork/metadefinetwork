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
    [Area("Admin")]
    public class CommingSoonController : Controller
    {

        public CommingSoonController()
        {
        }

        [HttpGet("Game")]
        public IActionResult Game()
        {
            return View();
        }
        [HttpGet("NFT")]
        public IActionResult NFT()
        {
            return View();
        }
        [HttpGet("Swap")]
        public IActionResult Swap()
        {
            return View();
        }
        [HttpGet("Staking")]
        public IActionResult Staking()
        {
            return View();
        }
        [HttpGet("Farming")]
        public IActionResult Farming()
        {
            return View();
        }
        [HttpGet("Exchange")]
        public IActionResult Exchange()
        {
            return View();
        }
        //[HttpGet("Support")]
        //public IActionResult Support()
        //{
        //    return View();
        //}
    }
}
