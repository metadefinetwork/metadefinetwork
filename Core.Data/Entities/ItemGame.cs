using Core.Data.Enums;
using Core.Data.Interfaces;
using Core.Infrastructure.SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Data.Entities
{
    [Table("ItemGame")]
    public class ItemGame : DomainEntity<Guid>
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public ItemType Type { get; set; }
        [Required]
        public ItemGroup GroupItem { get; set; }
        [Required]
        public Status Status { get; set; }
        [Required]
        public string ClassName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
