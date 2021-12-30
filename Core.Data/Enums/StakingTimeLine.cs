using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum StakingTimeLine
    {
        [Description("3 Month - 10% Interest")]
        ThreeMonth = 3,

        [Description("6 Month - 25% Interest")]
        SixMonth = 6,

        [Description("9 Month - 45% Interest")]
        NineMonth = 9,

        [Description("12 Month - 65% Interest")]
        TwelveMonth = 12
    }
}
