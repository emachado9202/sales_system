using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MovilShopStock.Models
{
    public class ProductModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Categoria")]
        [Required]
        public string Category { get; set; }

        [DisplayName("Producto")]
        [Required]
        public string Product { get; set; }

        [DisplayName("Precio Actual")]
        public string CurrentPrice { get; set; }

        [DisplayName("Precio Venta")]
        public string SalePrice { get; set; }

        [DisplayName("Existencia")]
        public int Stock { get; set; }

        [DisplayName("Última Modificación")]
        public string LatestUpdated { get; set; }

        [DisplayName("No Contar en Salida")]
        public bool NoCountOut { get; set; }

        [DisplayName("Es Accesorio?")]
        public bool isAccesory { get; set; }
    }
}