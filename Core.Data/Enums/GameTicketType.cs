using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum GameTicketType
    {
        [Description("Pay Lucky Round")]
        PayLuckyRound = 1,

        [Description("Swap From Wallet Invest")]
        SwapFromWalletInvest = 2,

        [Description("Swap From Wallet BNB BEP20")]
        SwapFromWalletBNBBEP20 = 3,

        [Description("Swap From Wallet BNB Affiliate")]
        SwapFromWalletBNBAffiliate = 4,

        [Description("Win Lucky Round")]
        WinLuckyRound = 5,

        [Description("Referral Bonus")]
        ReferralBonus = 6
    }
}
