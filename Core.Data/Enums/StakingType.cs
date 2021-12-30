using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum StakingType
    {
        [Description("In Process")]
        Process = 1,

        [Description("Finish")]
        Finish = 2
    }
}
