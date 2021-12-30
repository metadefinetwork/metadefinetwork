using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum WalletBNBTransactionType
    {
        [Description("Deposit")]
        Deposit = 1,

        [Description("Withdraw")]
        Withdraw = 2,

        [Description("Pay Private Sale")]
        PayPrivateSale = 3,

        [Description("Win Lucky Round")]
        WinLuckyRound = 4,

        [Description("Pay Ticket")]
        PayTicket = 5,

        [Description("Pay PreSale")]
        PayPreSale = 6,

        [Description("Pay Staking")]
        PayStaking = 7
    }
}
