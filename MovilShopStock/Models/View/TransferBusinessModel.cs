using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.View
{
    public class TransferBusinessModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Destino")]
        public string FromTo { get; set; }

        [DisplayName("Categoría")]
        public string Category { get; set; }

        [DisplayName("Producto")]
        public string ProductName { get; set; }

        [DisplayName("Cantidad")]
        public int Quantity { get; set; }

        public string User { get; set; }
        public string Date { get; set; }

        public bool Sent { get; set; }
    }
}