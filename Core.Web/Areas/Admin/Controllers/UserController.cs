using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Data.Entities;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Core.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITRONService _tronService;
        private readonly ITransactionService _transactionService;
        private readonly UserManager<AppUser> _userManager;

        public UserController(IUnitOfWork unitOfWork,
                              IRoleService roleService,
                              IUserService userService,
                              IAuthorizationService authorizationService,
                              ITRONService tronService,
                              ITransactionService transactionService,
                              UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _roleService = roleService;
            _userService = userService;
            _authorizationService = authorizationService;
            _tronService = tronService;
            _transactionService = transactionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            var roles = await _roleService.GetAllAsync();
            ViewBag.RoleId = new SelectList(roles, "Name", "Name");

            return View();
        }

        public IActionResult IndexTree()
        {
            if (!IsAdmin)
                return Redirect("/logout");

           
            return View();
        }

        [HttpGet]
        public IActionResult GetTreeAll()
        {
            var model = _userService.GetTreeAll();
            return new ObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        public IActionResult Customers()
        {
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            return View();
        }

        [HttpGet]
        public IActionResult GetAllCustomerPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllCustomerPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerSetting(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return new OkObjectResult(user);
        }

        public IActionResult StatementAllUser()
        {
            if (!IsAdmin)
                return Redirect("/logout");

            //var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Read);
            //if (result.Succeeded == false)
            //    return new RedirectResult("/Admin/Account/Login");

            return View();
        }

        [HttpGet]
        public IActionResult GetStatementAllUser(string keyword, int type)
        {
            var model = _userService.GetStatementUser(keyword, type);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var model = await _userService.GetById(id);

            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(AppUserViewModel userVm)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                if (userVm.Id == null)
                    await _userService.AddAsync(userVm);
                else
                    await _userService.UpdateAsync(userVm);

                return new OkObjectResult(userVm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomerSetting([FromBody] AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.Value.ToString());

            if (!userVm.Enabled2FA)
            {
                user.TwoFactorEnabled = false;
            }

            _unitOfWork.Commit();

            return new OkObjectResult(new GenericResult(true, "Update Success!"));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            try
            {
                if (!IsAdmin)
                    return Redirect("/logout");

                var model = await _userService.GetById(id);
                if (model.EmailConfirmed == true)
                {
                    return new OkObjectResult(new GenericResult(false, "Accounts confirmed email cannot be deleted."));
                }

                await _userService.DeleteAsync(id);

                return new OkObjectResult(new GenericResult(true, "Account deleted successfully"));
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResult(false, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!IsAdmin)
                return Redirect("/logout");

            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            else
            {
                await _userService.DeleteAsync(id);

                return new OkObjectResult(id);
            }
        }
    }
}
