using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("Stakings")]
    public class Staking : DomainEntity<int>
    {
        [Required]
        public int StakingPeriodId { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        [Required]
        public DateTime LastReceived { get; set; }
        [Required]
        public Guid AppUserId { get; set; }
        [Required]
        public int Period { get; set; }
        [Required]
        public decimal InterestRate { get; set; }
        [Required]
        public int NumberOfTimesReceived { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal ReceivedAmount { get; set; }
        [Required]
        public StakingType Status { get; set; }
        [Required]
        public Guid ItemGameId { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { set; get; }

        [ForeignKey("ItemGameId")]
        public virtual ItemGame ItemGame { set; get; }

        [ForeignKey("StakingPeriodId")]
        public virtual StakingPeriod StakingPeriod { set; get; }

    }
}
