using System.ComponentModel;

namespace MovilShopStock.Models.View
{
    public class TransferPrivateModel
    {
        public string DT_RowId { get; set; }

        [DisplayName("Destino")]
        public string FromTo { get; set; }

        [DisplayName("Monto de la Transferencia")]
        public string Amount { get; set; }

        public string Date { get; set; }

        public bool Sent { get; set; }
    }
}