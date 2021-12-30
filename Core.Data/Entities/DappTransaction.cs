using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Data.Entities
{
    [Table("DAppTransactions")]
    public class DAppTransaction : DomainEntity<Guid>
    {
        public string AddressTo { get; set; }
        public string AddressFrom { get; set; }
        public Guid? AppUserId { get; set; }
        public string Email { get; set; }
        public DAppTransactionState DAppTransactionState { get; set; }
        public DAppTransactionType Type { get; set; }
        public decimal BNBAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string BNBTransactionHash { get; set; }
        public string TokenTransactionHash { get; set; }
        public bool IsDevice { get; set; }
        public string WalletType { get; set; }
    }
}
