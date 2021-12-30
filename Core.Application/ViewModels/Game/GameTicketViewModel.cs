using Core.Application.ViewModels.System;
using Core.Data.Enums;
using System;
using System.Collections.Generic;

namespace Core.Application.ViewModels.Game
{
    public class GameTicketViewModel
    {
        public GameTicketViewModel()
        {
        }

        public int Id { get; set; }

        public string AddressFrom { get; set; }

        public string AddressTo { get; set; }

        public int Amount { get; set; }

        public GameTicketType Type { get; set; }
        public string TypeName { get; set; }
        public string AppUserName { get; set; }
        public string Sponsor { get; set; }

        public DateTime DateCreated { get; set; }

        public Guid AppUserId { get; set; }

        public AppUserViewModel AppUser { set; get; }
    }
}
