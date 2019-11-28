using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MovilShopStock.Models.Catalog
{
    public class TransferMoneyUser
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("UserFrom")]
        [Required]
        public string UserFrom_Id { get; set; }

        [ForeignKey("UserTo")]
        [Required]
        public string UserTo_Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        public User UserFrom { get; set; }
        public User UserTo { get; set; }
    }
}