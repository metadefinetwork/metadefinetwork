using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum WalletMVRTransactionType
    {
        [Description("Deposit")]
        Deposit = 1,

        [Description("Withdraw")]
        Withdraw = 2,

        [Description("Deposit Private Sale")]
        DepositPrivateSale = 3,

        [Description("Airdrop")]
        Airdrop = 4,

        [Description("Win Lucky Round")]
        WinLuckyRound = 5,

        [Description("Pay Ticket")]
        PayTicket = 6,

        [Description("Deposit PreSale")]
        DepositPreSale = 7,

        [Description("Pay Staking")]
        PayStaking = 8
    }
}
