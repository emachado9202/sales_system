using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovilShopStock.Models.Catalog
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal SalePrice { get; set; }

        public int In { get; set; }
        public int Out { get; set; }

        [ForeignKey("Category")]
        [Required]
        public Guid Category_Id { get; set; }

        [ForeignKey("Business")]
        public Guid? Business_Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public string User_Id { get; set; }

        public DateTime LastUpdated { get; set; }
        public bool NoCountOut { get; set; }

        public virtual Business Business { get; set; }
        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
        public virtual List<StockOut> StockOuts { get; set; }
        public virtual List<TransferBusinessProduct> TransferBusinessProducts { get; set; }
    }
}