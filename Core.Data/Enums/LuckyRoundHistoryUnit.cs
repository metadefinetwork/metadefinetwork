using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum LuckyRoundHistoryUnit
    {
        [Description("BNB")]
        BNB = 1,
        [Description("Ticket")]
        Ticket = 3,

        [Description("None")]
        None = 4
    }
}
