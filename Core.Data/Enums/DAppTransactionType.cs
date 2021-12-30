using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum DAppTransactionState
    {
        [Description("None")]
        None = 0,
        [Description("Requested")]
        Requested = 1,
        [Description("Confirmed")]
        Confirmed = 2,
        [Description("Rejected")]
        Rejected = 3,
        [Description("Failed")]
        Failed = 4,
    }

    public enum DAppTransactionType
    {
        [Description("Presale")]
        Presale = 0,
        [Description("Claim")]
        Claim = 1
    }
}
