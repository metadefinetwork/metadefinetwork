using Core.Application.Interfaces;
using Core.Application.ViewModels.System;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Entities;
using Core.Infrastructure.Interfaces;
using Core.Utilities.Constants;
using Core.Utilities.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Implementation
{
    public class UserService : IUserService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlockChainService _blockChainService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<DAppTransaction, Guid> _dappRepository;
        public UserService(UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IAuthenticationService authenticationService,
            IBlockChainService blockChainService, 
            IRepository<DAppTransaction, Guid> dappRepository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _blockChainService = blockChainService;
            _authenticationService = authenticationService;
            _dappRepository = dappRepository;
        }

        #region Customer

        public StatementUserViewModel GetStatementUser(string keyword, int type)
        {
            var model = new StatementUserViewModel();

            var query = _userManager.Users.Where(x => x.IsSystem == false && x.EmailConfirmed == true
            && (x.BNBBalance > 0 || x.MARBalance > 0
            || x.MVRBalance > 0 ));

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Email.Contains(keyword)
                || x.Sponsor.Value.ToString().Contains(keyword)
                || x.BNBBEP20PublishKey.Contains(keyword)
                || x.BEP20PublishKey.Contains(keyword));

            //if (type == 1)
            //    query = query.Where(x => x.LockBalance > 0);
            //else if (type == 2)
            //    query = query.Where(x => x.InvestBalance > 0);
            //else if (type == 3)
            //    query = query.Where(x => x.MainBalance > 0);
            //else if (type == 4)
            //    query = query.Where(x => x.BNBAffiliateBalance > 0);
            //else if (type == 5)
            //    query = query.Where(x => x.TokenCommissionBalance > 0);
            //else if (type == 6)
            //    query = query.Where(x => x.TokenAffiliateBalance > 0);

            query = query.Where(x => CommonConstants.MemberAccessDenied.Where(ma => ma.Email.ToLower() == x.Email.ToLower()).Count() == 0);

            model.AppUsers = query.Select(x => new AppUserViewModel()
            {
                Id = x.Id,
                Sponsor = $"{x.Sponsor}",
                Email = x.Email,
                BNBBalance = x.BNBBalance,
                MARBalance = x.MARBalance,
                MVRBalance = x.MVRBalance,
                StakingBalance = x.StakingBalance
            }).ToList();

            model.TotalMember = model.AppUsers.Count();
            model.TotalBNBBalance = model.AppUsers.Sum(x => x.BNBBalance);
            model.TotalMVRBalance = model.AppUsers.Sum(x => x.MVRBalance);
            model.TotalMARBalance = model.AppUsers.Sum(x => x.MARBalance);

            return model;
        }

        public PagedResult<AppUserViewModel> GetAllCustomerPagingAsync(string keyword, int page, int pageSize)
        {
            var query = _userManager.Users.Where(x => x.IsSystem == false);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Email.Contains(keyword)
                || x.Sponsor.Value.ToString().Contains(keyword)
                || x.BNBBEP20PublishKey.Contains(keyword)
                || x.BEP20PublishKey.Contains(keyword));

            int totalRow = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    EmailConfirmed = x.EmailConfirmed,
                    Sponsor = $"{x.Sponsor}",
                    Email = x.Email,
                    BNBBalance = x.BNBBalance,
                    MARBalance = x.MARBalance,
                    MVRBalance = x.MVRBalance,
                    BNBBEP20PublishKey = x.BNBBEP20PublishKey,
                    BEP20PublishKey = x.BEP20PublishKey,
                    StakingBalance = x.StakingBalance,
                    Status = x.Status,
                    DateCreated = x.DateCreated,
                }).ToList();

            var paginationSet = new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public async Task<NetworkViewModel> GetNetworkInfo(string id)
        {
            var customer = await _userManager.FindByIdAsync(id);
            if (customer != null)
            {
                var model = new NetworkViewModel
                {
                    Email = customer.Email,
                    Member = customer.UserName,
                    Sponsor = $"MTD{customer.Sponsor}",
                    EmailConfirmed = customer.EmailConfirmed,
                    ReferalLink = $"https://metadefi.network/register?sponsor=MTD{customer.Sponsor}",
                    CreatedDate = customer.DateCreated
                };
                //model.ReferralAddress = customer.ReferralAddress;
                return model;
            }
            else {
                var model = new NetworkViewModel
                {
                    ReferalLink = $"https://metadefi.network/register?sponsor={id}",
                    CreatedDate = DateTime.Now
                };
                return model;
            }
           
        }

        public async Task<NetworkViewModel> GetTotalNetworkInfo(string id)
        {
            var model = new NetworkViewModel();

            var customer = await _userManager.FindByIdAsync(id);

            var userList = _userManager.Users.Where(x => x.IsSystem == false);

            var f1Customers = userList.Where(x => x.ReferralId == customer.Id);
            var f1CustomerCount = f1Customers.Count();
            model.TotalF1 = f1CustomerCount;
            model.TotalMember = f1CustomerCount;

            if (f1CustomerCount > 0)
            {
                var f1CustomerIDs = f1Customers.Select(x => x.Id).ToList();

                var f2Customers = userList.Where(x => f1CustomerIDs.Contains(x.ReferralId.Value));
                var f2CustomerCount = f2Customers.Count();
                model.TotalF2 = f2CustomerCount;
                model.TotalMember += f2CustomerCount;

                if (f2CustomerCount > 0)
                {
                    var f2CustomerIDs = f2Customers.Select(x => x.Id).ToList();
                    var f3Customers = userList.Where(x => f2CustomerIDs.Contains(x.ReferralId.Value));
                    var f3CustomerCount = f3Customers.Count();
                    model.TotalF3 = f3CustomerCount;
                    model.TotalMember += f3CustomerCount;

                    if (f3CustomerCount>0)
                    {
                        var f3CustomerIDs = f3Customers.Select(x => x.Id).ToList();
                        var f4Customers = userList.Where(x => f3CustomerIDs.Contains(x.ReferralId.Value));
                        var f4CustomerCount = f4Customers.Count();
                        model.TotalF4 = f4CustomerCount;
                        model.TotalMember += f4CustomerCount;

                        if (f4CustomerCount > 0)
                        {
                            var f4CustomerIDs = f4Customers.Select(x => x.Id).ToList();
                            var f5Customers = userList.Where(x => f4CustomerIDs.Contains(x.ReferralId.Value));
                            var f5CustomerCount = f5Customers.Count();
                            model.TotalF5 = f5CustomerCount;
                            model.TotalMember += f5CustomerCount;
                        }
                    }
                }
            }

            return model;
        }

        //public async Task<NetworkViewModel> GetTotalNetworkInfo(string id)
        //{
        //    var model = new NetworkViewModel();

        //    var customer = await _userManager.FindByNameAsync(id);

        //    var userList = _userManager.Users.Where(x => x.IsSystem == false);

        //    var f1Customers = userList.Where(x => x.MainPublishKey == customer.UserName);
        //    var f1CustomerCount = f1Customers.Count();
        //    model.TotalF1 = f1CustomerCount;
        //    model.TotalMember = f1CustomerCount;

        //    if (f1CustomerCount > 0)
        //    {
        //        var f1CustomerIDs = f1Customers.Select(x => x.UserName).ToList();

        //        var f2Customers = userList.Where(x => f1CustomerIDs.Contains(x.MainPublishKey));
        //        var f2CustomerCount = f2Customers.Count();
        //        model.TotalF2 = f2CustomerCount;
        //        model.TotalMember += f2CustomerCount;

        //        if (f2CustomerCount > 0)
        //        {
        //            var f2CustomerIDs = f2Customers.Select(x => x.UserName).ToList();
        //            var f3Customers = userList.Where(x => f2CustomerIDs.Contains(x.MainPublishKey));
        //            var f3CustomerCount = f3Customers.Count();
        //            model.TotalF3 = f3CustomerCount;
        //            model.TotalMember += f3CustomerCount;

        //            if (f3CustomerCount > 0)
        //            {
        //                var f3CustomerIDs = f2Customers.Select(x => x.UserName).ToList();
        //                var f4Customers = userList.Where(x => f3CustomerIDs.Contains(x.MainPublishKey));
        //                var f4CustomerCount = f4Customers.Count();
        //                model.TotalF4 = f4CustomerCount;
        //                model.TotalMember += f4CustomerCount;

        //                if (f4CustomerCount > 0)
        //                {
        //                    var f4CustomerIDs = f3Customers.Select(x => x.UserName).ToList();
        //                    var f5Customers = userList.Where(x => f3CustomerIDs.Contains(x.MainPublishKey));
        //                    var f5CustomerCount = f5Customers.Count();
        //                    model.TotalF5 = f5CustomerCount;
        //                    model.TotalMember += f5CustomerCount;
        //                }
        //            }
        //        }
        //    }

        //    return model;
        //}

        //public PagedResult<AppUserViewModel> GetCustomerReferralPagingAsync(string userName, int refIndex, string keyword, int page, int pageSize)
        //{
        //    IQueryable<AppUser> dataCustomers = null;
        //    var userList = _userManager.Users.Where(x => x.IsSystem == false);

        //    if (!string.IsNullOrEmpty(keyword))
        //        userList = userList.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));

        //    var f1Customers = userList.Where(x => x.MainPublishKey == userName);
        //    if (refIndex == 1)
        //    {
        //        dataCustomers = f1Customers;
        //    }
        //    else
        //    {
        //        var f1Ids = f1Customers.Select(x => x.UserName).ToList();
        //        var f2Customers = userList.Where(x => f1Ids.Contains(x.MainPublishKey));
        //        if (refIndex == 2)
        //        {
        //            dataCustomers = f2Customers;
        //        }
        //        else
        //        {
        //            var f2Ids = f2Customers.Select(x => x.UserName).ToList();
        //            var f3Customers = userList.Where(x => f2Ids.Contains(x.MainPublishKey));
        //            if (refIndex == 3)
        //            {
        //                dataCustomers = f3Customers;
        //            }
        //            else
        //            {

        //                var f3Ids = f3Customers.Select(x => x.UserName).ToList();
        //                var f4Customers = userList.Where(x => f3Ids.Contains(x.MainPublishKey));
        //                if (refIndex == 4)
        //                {
        //                    dataCustomers = f4Customers;
        //                }
        //                else
        //                {
        //                    var f4Ids = f4Customers.Select(x => x.UserName).ToList();
        //                    var f5Customers = userList.Where(x => f4Ids.Contains(x.MainPublishKey));
        //                    if (refIndex == 5)
        //                    {
        //                        dataCustomers = f5Customers;
        //                    }
        //                }

        //            }
        //        }
        //    }

        //    int totalRow = dataCustomers.Count();
        //    var data = new List<AppUserViewModel>();
        //    if (totalRow > 0)
        //        data = dataCustomers.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
        //        .Select(x => new AppUserViewModel()
        //        {
        //            Id = x.Id,
        //            UserName = x.UserName,
        //            EmailConfirmed = x.EmailConfirmed,
        //            Email = x.Email,
        //            Status = x.Status,
        //            ReferralId = x.ReferralId,
        //            DateCreated = x.DateCreated
        //        }).ToList();

        //    return new PagedResult<AppUserViewModel>()
        //    {
        //        Results = data,
        //        CurrentPage = page,
        //        RowCount = totalRow,
        //        PageSize = pageSize
        //    };
        //}

        public PagedResult<AppUserViewModel> GetCustomerReferralPagingAsync(string customerId, int refIndex, string keyword, int page, int pageSize)
        {
            IQueryable<AppUser> dataCustomers = null;
            var userList = _userManager.Users.Where(x => x.IsSystem == false);
            var f1Customers = userList.Where(x => x.ReferralId == new Guid(customerId));
            if (refIndex == 1)
            {
                dataCustomers = f1Customers;
            }
            else
            {
                var f1Ids = f1Customers.Select(x => x.Id).ToList();
                var f2Customers = userList.Where(x => f1Ids.Contains(x.ReferralId.Value));
                if (refIndex == 2)
                {
                    dataCustomers = f2Customers;
                }
                else
                {
                    var f2Ids = f2Customers.Select(x => x.Id).ToList();
                    var f3Customers = userList.Where(x => f2Ids.Contains(x.ReferralId.Value));
                    if (refIndex == 3)
                    {
                        dataCustomers = f3Customers;
                    }
                    else
                    {

                        var f3Ids = f3Customers.Select(x => x.Id).ToList();
                        var f4Customers = userList.Where(x => f3Ids.Contains(x.ReferralId.Value));
                        if (refIndex == 4)
                        {
                            dataCustomers = f4Customers;
                        }
                        else
                        {
                            var f4Ids = f4Customers.Select(x => x.Id).ToList();
                            var f5Customers = userList.Where(x => f4Ids.Contains(x.ReferralId.Value));
                            if (refIndex == 5)
                            {
                                dataCustomers = f5Customers;
                            }
                        }

                    }
                }
            }
            if (!string.IsNullOrEmpty(keyword))
                dataCustomers = dataCustomers.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));
            int totalRow = dataCustomers.Count();
            var data = dataCustomers.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Sponsor = $"MTD{x.Sponsor}",
                    EmailConfirmed = x.EmailConfirmed,
                    Email = x.Email,
                    Status = x.Status,
                    ReferralId = x.ReferralId,
                    DateCreated = x.DateCreated,
                }).ToList();

            var userEmails = data.Select(x => x.Email);
            var claimedUsers = _dappRepository.FindAll(d => userEmails.Contains(d.Email) && d.Type == Data.Enums.DAppTransactionType.Claim && d.DAppTransactionState == Data.Enums.DAppTransactionState.Confirmed).Select(d => d.Email).ToList();

            foreach (var item in data)
            {
                item.HasClaimed = claimedUsers.Any(u => u == item.Email);
            }

            return new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };
        }

        #endregion Customer

        #region User System

        public PagedResult<AppUserViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            var query = _userManager.Users.Where(x => x.IsSystem);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.UserName.Contains(keyword) || x.Email.Contains(keyword));

            int totalRow = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AppUserViewModel()
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    Status = x.Status,
                    DateCreated = x.DateCreated
                }).ToList();

            var paginationSet = new PagedResult<AppUserViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public List<AppUserTreeViewModel> GetTreeAll()
        {
            var listData = _userManager.Users.OrderBy(x => x.Sponsor)
                .Select(x => new AppUserTreeViewModel()
                {
                    id = x.Id,
                    text = $"{x.Sponsor}-{x.Email}-{x.EmailConfirmed}-{x.MARBalance}-{x.MVRBalance}-{x.BNBBalance}-{x.DateCreated}",
                    icon = "fa fa-users text-success",
                    state = new AppUserTreeState { opened = true },
                    data = new AppUserTreeData
                    {
                        referralId = x.ReferralId,
                        rootId = x.Id,
                    }
                });

            if (listData.Count() == 0)
                return new List<AppUserTreeViewModel>();

            var groups = listData.AsEnumerable().GroupBy(i => i.data.referralId);

            var roots = groups.FirstOrDefault(g => g.Key.HasValue == false).ToList();

            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue)
                    .ToDictionary(g => g.Key.Value, g => g.ToList());

                for (int i = 0; i < roots.Count; i++)
                    AddChildren(roots[i], dict);
            }
            return roots;
        }

        private void AddChildren(AppUserTreeViewModel root, IDictionary<Guid, List<AppUserTreeViewModel>> source)
        {
            if (source.ContainsKey(root.id))
            {
                root.children = source[root.id].ToList();
                for (int i = 0; i < root.children.Count; i++)
                    AddChildren(root.children[i], source);
            }
            else
            {
                root.icon = "fa fa-user text-danger";
                root.children = new List<AppUserTreeViewModel>();
            }
        }

        public async Task<AppUserViewModel> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var userVm = new AppUserViewModel()
            {
                Id = user.Id,
                EmailConfirmed = user.EmailConfirmed,
                ReferralId = user.ReferralId,
                Enabled2FA = user.TwoFactorEnabled,
                BEP20PrivateKey = user.BEP20PrivateKey,
                BEP20PublishKey = user.BEP20PublishKey,
                BNBBalance = user.BNBBalance,
                MARBalance = user.MARBalance,
                MVRBalance = user.MVRBalance,
                StakingBalance = user.StakingBalance,
                StakingInterest = user.StakingInterest,
                BNBBEP20PublishKey = user.BNBBEP20PublishKey,
                ReferalLink = $"https://metadefi.network/register?sponsor=MTD{user.Sponsor}",
                Sponsor = $"MTD{user.Sponsor}",
                IsSystem = user.IsSystem,
                ByCreated = user.ByCreated,
                ByModified = user.ByModified,
                DateModified = user.DateModified,
                UserName = user.UserName,
                Email = user.Email,
                Status = user.Status,
                DateCreated = user.DateCreated,
                Roles = roles.ToList()
            };

            if (!user.TwoFactorEnabled)
            {
                userVm.AuthenticatorCode = await _authenticationService.GetAuthenticatorKey(user);
            }

            return userVm;
        }

        public async Task<bool> AddAsync(AppUserViewModel userVm)
        {
            var user = new AppUser()
            {
                UserName = userVm.UserName,
                Email = userVm.Email,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                StakingBalance = 0,
                StakingInterest = 0,
                IsSystem = true
            };

            var result = await _userManager.CreateAsync(user, userVm.Password);
            if (result.Succeeded && userVm.Roles.Count > 0)
            {
                var appUser = await _userManager.FindByNameAsync(user.UserName);
                if (appUser != null)
                {
                    await _userManager.AddToRolesAsync(appUser, userVm.Roles.ToArray());
                }
            }

            return result.Succeeded;
        }

        public async Task UpdateAsync(AppUserViewModel userVm)
        {
            var user = await _userManager.FindByIdAsync(userVm.Id.ToString());

            //Remove current roles in db
            var currentRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, userVm.Roles.Except(currentRoles).ToArray());
            if (result.Succeeded)
            {
                string[] needRemoveRoles = currentRoles.Except(userVm.Roles).ToArray();
                await _userManager.RemoveFromRolesAsync(user, needRemoveRoles);

                user.Status = userVm.Status;
                user.Email = userVm.Email;
                user.DateModified = DateTime.Now;

                await _userManager.UpdateAsync(user);
            }
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }

        #endregion User System
    }
}
