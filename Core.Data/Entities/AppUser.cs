using Core.Data.Enums;
using Core.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("AppUsers")]
    public class AppUser : IdentityUser<Guid>, IDateTracking, ISwitchable
    {
        public int? Sponsor { get; set; }
        public Guid? ReferralId { get; set; }
        public string BNBBEP20PublishKey { get; set; }
        public string BEP20PrivateKey { get; set; }
        public string BEP20PublishKey { get; set; }
        
        public decimal BNBBalance { get; set; }
        public decimal MARBalance { get; set; }
        public decimal MVRBalance { get; set; }

        public decimal StakingBalance { get; set; }
        public decimal StakingInterest { get; set; }

        public int TicketBalance { get; set; }

        public bool IsSystem { get; set; } = false;
        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string ByCreated { get; set; }
        public string ByModified { get; set; }

        public virtual ICollection<CustomerTransaction> CustomerTransactions { set; get; }
        public virtual ICollection<Support> Supports { set; get; }
        public virtual ICollection<TicketTransaction> TicketTransactions { set; get; }
        public virtual ICollection<WalletBNBTransaction> WalletBNBTransactions { set; get; }
        public virtual ICollection<WalletMVRTransaction> WalletMVRTransactions { set; get; }
        public virtual ICollection<LuckyRoundHistory> LuckyRoundHistories { get; set; }
        public virtual ICollection<GameTicket> GameTickets { get; set; }
        public virtual ICollection<Staking> Stakings { get; set; }
        public virtual ICollection<StakingReward> StakingRewards { get; set; }
    }
}
