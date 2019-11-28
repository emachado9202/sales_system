using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.Catalog
{
    public class TransferBusinessProduct
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ProductFrom")]
        [Required]
        public Guid ProductFrom_Id { get; set; }

        [ForeignKey("ProductTo")]
        [Required]
        public Guid ProductTo_Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public string User_Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Quantity { get; set; }

        public User User { get; set; }
        public Product ProductFrom { get; set; }
        public Product ProductTo { get; set; }
    }
}