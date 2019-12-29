using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [Range(1, 1000, ErrorMessage = "La cantidad tiene que ser entre 1 y 1000")]
        public int Quantity { get; set; }

        [DisplayName("Descripción")]
        public string Description { get; set; }

        [DisplayName("Proveedor")]
        public string Provider { get; set; }

        [DisplayName("Total Compra")]
        public string TotalShop { get { return (decimal.Parse(ShopPrice) * Quantity).ToString("#,##0.00"); } }
    }
}