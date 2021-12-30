using Core.Application.ViewModels.Blog;
using Core.Application.ViewModels.Common;
using Core.Application.ViewModels.Game;
using Core.Application.ViewModels.Valuesshare;
using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.System
{
    public class AppUserViewModel
    {
        public AppUserViewModel()
        {
            Roles = new List<string>();
            Transactions = new List<TransactionViewModel>();
            Supports = new List<SupportViewModel>();
            WalletBNBTransactions = new List<WalletBNBTransactionViewModel>();
            TicketTransactions = new List<TicketTransactionViewModel>();
            WalletMVRTransactions = new List<WalletMVRTransactionViewModel>();
            LuckyRoundHistories = new List<LuckyRoundHistoryViewModel>();
            GameTickets = new List<GameTicketViewModel>();
            Stakings = new List<StakingViewModel>();
            StakingRewards = new List<StakingRewardViewModel>();
        }

        public Guid? Id { set; get; }
        public Guid? ReferralId { get; set; }
        public string Sponsor { get; set; }
        public bool IsSystem { get; set; }
        public string Email { set; get; }
        public string UserName { set; get; }
        public string Password { set; get; }
        public bool EmailConfirmed { get; set; }
        public bool Enabled2FA { get; set; }
        public string AuthenticatorCode { get; set; }
        public string ReferalLink { get; set; }

        public string BNBBEP20PublishKey { get; set; }

        public string BEP20PrivateKey { get; set; }
        public string BEP20PublishKey { get; set; }
        public decimal BNBBalance { get; set; }
        public decimal MARBalance { get; set; }
        public decimal MVRBalance { get; set; }

        public decimal StakingBalance { get; set; }
        public decimal StakingInterest { get; set; }

        public int TicketBalance { get; set; }

        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string ByCreated { get; set; }
        public string ByModified { get; set; }
        public bool HasClaimed { get; set; }
        public List<string> Roles { get; set; }
        public List<TransactionViewModel> Transactions { set; get; }
        public List<SupportViewModel> Supports { set; get; }
        public List<TicketTransactionViewModel> TicketTransactions { set; get; }
        public List<WalletBNBTransactionViewModel> WalletBNBTransactions { set; get; }
        public List<WalletMVRTransactionViewModel> WalletMVRTransactions { set; get; }
        public List<LuckyRoundHistoryViewModel> LuckyRoundHistories { get; set; }
        public List<GameTicketViewModel> GameTickets { get; set; }
        public List<StakingViewModel> Stakings { get; set; }
        public List<StakingRewardViewModel> StakingRewards { get; set; }
    }
}
