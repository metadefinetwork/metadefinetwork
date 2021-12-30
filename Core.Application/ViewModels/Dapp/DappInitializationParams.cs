using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.Dapp
{
    public class DappInitializationParams
    {
        public decimal BNBAmount { get; set; }

        [Required]
        public string Address { get; set; }
        public string Email { get; set; }
        public bool IsDevice { get; set; }
        public string WalletType { get; set; }
        public string AppUserId { get; set; }
    }
}
