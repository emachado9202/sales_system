using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models
{
    public class StockInModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Categoría")]
        public string Category { get; set; }

        [DisplayName("Nombre Producto")]
        [Required]
        public string ProductName { get; set; }

        [DisplayName("Fecha de Registro")]
        public string Date { get; set; }

        [DisplayName("Usuario")]
        public string User { get; set; }

        [DisplayName("Precio Compra")]
        public string ShopPrice { get; set; }

        [DisplayName("Cantidad Compra")]
        public int Quantity { get; set; }

        [DisplayName("Total Compra")]
        public decimal TotalShop { get { return decimal.Parse(ShopPrice) * Quantity; } }
    }
}