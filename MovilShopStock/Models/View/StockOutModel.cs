using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models
{
    public class StockOutModel
    {
        public string Id { get; set; }

        [DisplayName("Categoría")]
        public string Category { get; set; }

        [DisplayName("Nombre Producto")]
        [Required]
        public string ProductName { get; set; }

        [DisplayName("Fecha de Registro")]
        public string Date { get; set; }

        [DisplayName("Usuario")]
        public string User { get; set; }

        [DisplayName("Recibido")]
        public string Receiver { get; set; }

        public bool Receivered { get; set; }

        [DisplayName("Precio Venta")]
        public decimal SalePrice { get; set; }

        [DisplayName("Cantidad Venta")]
        public int Quantity { get; set; }

        [DisplayName("Venta Total")]
        public decimal TotalSale { get { return SalePrice * Quantity; } }

        [DisplayName("Ganancia")]
        public decimal Gain { get; set; }

        [DisplayName("Ganancia Total")]
        public decimal TotalGain { get { return Gain * Quantity; } }
    }
}