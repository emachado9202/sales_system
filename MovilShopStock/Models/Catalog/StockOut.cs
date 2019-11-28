using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovilShopStock.Models.Catalog
{
    public class StockOut
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Product")]
        [Required]
        public Guid Product_Id { get; set; }

        public DateTime Date { get; set; }

        [DataType(DataType.Currency)]
        public decimal SalePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal Gain { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("User")]
        [Required]
        public string User_Id { get; set; }

        [ForeignKey("Receiver")]
        public string Receiver_Id { get; set; }

        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
        public virtual User Receiver { get; set; }
    }
}