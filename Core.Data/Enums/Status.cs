using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum Status
    {
        [Description("None Active")]
        NoneActive = -1,
        [Description("In Active")]
        InActive = 0,
        [Description("Active")]
        Active = 1
    }
}
