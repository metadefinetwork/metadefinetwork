﻿using Core.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.System
{
    public class WalletMARTransactionViewModel
    {

        public int Id { get; set; }
        public string TransactionHash { get; set; }

        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }

        public decimal Fee { get; set; }
        public decimal FeeAmount { get; set; }
     
        public decimal Amount { get; set; }
        public decimal AmountReceive { get; set; }

        public WalletMARTransactionType Type { get; set; }
        public string TypeName { get; set; }

        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }
        public string AppUserName { get; set; }
        public string Email { get; set; }
        public string Sponsor { get; set; }
    }

}
