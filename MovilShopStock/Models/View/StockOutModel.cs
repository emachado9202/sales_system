using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MovilShopStock.Models
{
    public class StockOutModel
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

        [DisplayName("Recibido")]
        public string Receiver { get; set; }

        public bool Receivered { get; set; }

        [DisplayName("Precio Venta")]
        public string SalePrice { get; set; }

        [DisplayName("Cantidad Venta")]
        [Range(1, 1000, ErrorMessage = "La cantidad tiene que ser entre 1 y 1000")]
        public int Quantity { get; set; }

        [DisplayName("Descripción")]
        public string Description { get; set; }

        [DisplayName("Venta Total")]
        public string TotalSale { get { return (decimal.Parse(SalePrice.Replace(".", ",")) * Quantity).ToString("#,##0.00"); } }

        [DisplayName("Ganancia")]
        public string Gain { get; set; }

        [DisplayName("Ganancia Total")]
        public string TotalGain { get { return (decimal.Parse(Gain.Replace(".", ",")) * Quantity).ToString("#,##0.00"); } }

        public List<AccesoryModel> Accesories { get; set; }
        public List<string> AccesoriesIds { get; set; }
    }

    public class AccesoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
}