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

        [DisplayName("Entrada")]
        public int In { get; set; }

        [DisplayName("Salida")]
        public int Out { get; set; }

        [DisplayName("Existencia")]
        public int Exist { get { return In - Out; } }

        [DisplayName("Última Modificación")]
        public string LatestUpdated { get; set; }

        [DisplayName("No Contar en Salida")]
        public bool NoCountOut { get; set; }
    }
}