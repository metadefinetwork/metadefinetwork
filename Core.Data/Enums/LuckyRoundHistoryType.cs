using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum LuckyRoundHistoryType
    {
        [Description("Pay Lucky Round")]
        PayLuckyRound = 1,

        [Description("Good Luck")]
        GoodLuck = 6,

        [Description("Win Ticket")]
        WinTicket = 8,

        [Description("Win 0.5 BNB")]
        Win05BNB = 9
    }
}
