using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Application.ViewModels.BlockChain
{
    public class WalletViewModel
    {
        public string AuthenticatorCode { get; set; }
        public bool Enabled2FA { get; set; }

        public string BNBBEP20PublishKey { get; set; }

        public string BEP20PrivateKey { get; set; }
        public string BEP20PublishKey { get; set; }

        public decimal BNBBalance { get; set; }
        public decimal MVRBalance { get; set; }
        public decimal MARBalance { get; set; }

        public decimal StakingBalance { get; set; }
        public decimal StakingInterest { get; set; }
    }
}
