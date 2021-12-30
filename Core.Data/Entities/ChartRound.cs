using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("ChartRounds")]
    public class ChartRound : DomainEntity<int>
    {
        [Required]
        public decimal DistributedToken { get; set; }

        [Required]
        public decimal Total { get; set; }

        [Required]
        public ChartRoundType Type { get; set; }

        [Required]
        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime DateUpdated { get; set; }
    }
}
