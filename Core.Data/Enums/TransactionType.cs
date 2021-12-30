using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum TransactionType
    {
        [Description("Airdrop")]
        Airdrop = 0,

        [Description("Pay Private Sale")]
        PayPrivateSale = 4,

        [Description("Pay PreSale")]
        PayPreSale = 5
    }
}
