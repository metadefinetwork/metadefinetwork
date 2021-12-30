using Core.Data.Enums;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entities
{
    [Table("ItemGameUsers")]
    public class ItemGameUser: DomainEntity<int>
    {
        public DateTime DateCreated { get; set; }
        public Guid ItemGameId { get; set; }
        public Guid AppUserId { get; set; }
        [ForeignKey("ItemGameId")]
        public virtual ItemGame ItemGame { get; set; }
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; }
        public decimal Amount { get; set; }
        public ItemType ItemType { get; set; }
        public Status Status { get; set; }
        public ItemTypeUser Type { get; set; }
    }
}
