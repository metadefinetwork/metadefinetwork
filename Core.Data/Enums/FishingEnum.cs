using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Core.Data.Enums
{
    public enum ItemType
    {
        [Description("Fishing Rod")]
        ROD = 1,
        [Description("Fishing Bait")]
        BAIT = 2,
        [Description("Fishing Hook")]
        HOOK = 3,
        [Description("Fishing Tank")]
        TANK = 4,
        [Description("Fish Line")]
        LINE = 6,
        [Description("Fish")]
        FISH = 5,
        
    }
    public enum ItemGroup
    {
        [Description("Staking")]
        Staking = 1,
        [Description("Game")]
        Game = 2,
    }
    public enum ItemTypeUser
    {
        [Description("Buy")]
        Buy = 1,
        [Description("Fishing")]
        Fishing = 2,
    }
    public enum StatusLake
    {
        [Description("Active")]
        Active = 1,
        [Description("Burn")]
        Burn = 2,
    }
}
