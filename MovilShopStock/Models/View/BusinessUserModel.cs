using System.ComponentModel;

namespace MovilShopStock.Models
{
    public class BusinessUserModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [DisplayName("Dinero")]
        public string Cash { get; set; }

        [DisplayName("Última Actualización")]
        public string LastUpdated { get; set; }
    }
}