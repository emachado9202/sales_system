using System;
using System.ComponentModel.DataAnnotations;

namespace MovilShopStock.Models.Catalog
{
    public class Business
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Photo { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsPrimary { get; set; }
    }
}