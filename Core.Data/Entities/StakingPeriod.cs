using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("StakingPeriods")]
    public class StakingPeriod : DomainEntity<int>
    {
        [Required]
        public int Period { get; set; }
        [Required]
        public decimal Interest { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
