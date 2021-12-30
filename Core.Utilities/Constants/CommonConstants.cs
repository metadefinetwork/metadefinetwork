using System.Collections.Generic;

namespace Core.Utilities.Constants
{
    public class CommonConstants
    {
        public class MemberSystem
        {
            public string Email { get; set; }
            public decimal Amount { get; set; }
        }

        public static List<MemberSystem> MemberAccessDenied = new List<MemberSystem>
        {
        };

        public const string DefaultFooterId = "DefaultFooterId";
        public const string DefaultContactId = "default";
        public const string CartSession = "CartSession";
        public const string ProductTag = "Product";
        public const string BlogTag = "Blog";

        public class AppRole
        {
            public const string AdminRole = "Admin";
        }

        public class UserClaims
        {
            public const string Roles = "Roles";
        }

        //#endregion BEP20 TestNet
        #region BEP20 MainNet

        public const string BEP20Url = "https://quiet-weathered-voice.bsc.quiknode.pro/888/";

        public const string BEP20TokenContract = "0xC7993a3E3b83DE0F61b39647284B10c7d91ce5e4";
        public const int BEP20TokenDP = 18;

        public const int ChainId = 56;
        #endregion BEP20 MainNet

        #region ERC20

        public const string ERC20Url = "*****";
        public const string ERC20VITONGPrivateKey = "*****";
        public const string ERC20VITONGPublishKey = "*****";
        public const string ERC20TGContract = "*****";
        public const int ERC20TGDP = 18;
        public const string ERC20USDTContract = "*****";
        public const int ERC20USDTDP = 6;

        #endregion ERC20

        #region TRC20

        public const string TRC20Url = "*****";
        public const string TRC20VITONGPrivateKey = "*****";
        public const string TRC20VITONGPublishKey = "*****";
        public const string TRC20USDTContract = "*****";
        public const string TRC20TGContract = "*****";
        public const string TRONApiKey = "*****";

        #endregion TRC20

        public static string[] WhiteListMembers = new string[]
        {
            "888",
            "888"
        };
    }
}
