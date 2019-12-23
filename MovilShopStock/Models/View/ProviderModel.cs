using System.ComponentModel;

namespace MovilShopStock.Models.View
{
    public class ProviderModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; }

        [DisplayName("Contacto (Email/Teléfono)")]
        public string Contact { get; set; }

        public string LastUpdated { get; set; }
    }
}