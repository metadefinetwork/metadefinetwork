using System;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.BotTelegram;
using Core.Data.Entities;
using Core.Data.Enums;
using Core.Extensions;
using Core.Services;
using Core.Utilities.Dtos;
using Core.Utilities.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Core.Areas.Admin.Controllers
{
    public class TicketTransactionController : BaseController
    {
        private readonly ITicketTransactionService _ticketTransactionService;
        private readonly IBlockChainService _blockChainService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public TicketTransactionController(
            ITicketTransactionService ticketTransactionService,
            UserManager<AppUser> userManager,
            IEmailSender emailSender,
            IBlockChainService blockChainService
            )
        {
            _ticketTransactionService = ticketTransactionService;
            _userManager = userManager;
            _emailSender = emailSender;
            _blockChainService = blockChainService;
        }

        public IActionResult Index()
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() != "admin")
                return Redirect("/logout");

            return View();
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _ticketTransactionService.GetAllPaging(keyword, "", page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveTicket(int id)
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() != "admin")
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    var result = await _ticketTransactionService.Approved(id);

                    if (result.Success)
                    {
                        //Report Withdraw
                        try
                        {
                            var ticketTransaction = result.Data as TicketTransaction;

                            var appUser = await _userManager.FindByIdAsync(ticketTransaction.AppUserId.ToString());
                            if (appUser != null)
                            {
                                var usd = _blockChainService.GetCurrentPrice("BNB", "USD");
                                var parameters = new WithdrawMessageParam
                                {
                                    Amount = ticketTransaction.AmountReceive,
                                    WithDrawTime = DateTime.Now,
                                    Email = appUser.Email,
                                    UserId = appUser.Sponsor,
                                    Wallet = ticketTransaction.AddressTo,
                                    Currency = ticketTransaction.Unit.ToString(),
                                    Rate = ticketTransaction.Unit == TicketTransactionUnit.BNB ? usd : 0,
                                    Fee = ticketTransaction.FeeAmount,
                                };

                                if (appUser.ReferralId.HasValue)
                                {
                                    var refferal = await _userManager.FindByIdAsync(appUser.ReferralId.ToString());

                                    parameters.SponsorEmail = refferal.Email;
                                    parameters.SponsorId = refferal.Sponsor;
                                }

                                var message = _emailSender.BuildReportWITHDRAWMessage(parameters);

                                await _emailSender.TrySendEmailAsync(appUser.Email, 
                                    $"Withdraw Successful {DateTime.UtcNow.ToddMMyyyyHHmmss()}(UTC)", message);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Ignore error from sending email.
                        }
                    }

                    return new OkObjectResult(new GenericResult(result.Success, result.Message));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectTicket(int id)
        {
            var roleName = User.GetSpecificClaim("RoleName");
            if (roleName.ToLower() != "admin")
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                try
                {
                    await _ticketTransactionService.Rejected(id);
                    return new OkObjectResult(new GenericResult(true, "Reject ticket is success"));
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new GenericResult(false, ex.Message));
                }
            }
        }
    }
}
