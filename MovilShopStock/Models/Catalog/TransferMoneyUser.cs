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

        [ForeignKey("BusinessFrom")]
        public Guid BusinessFrom_Id { get; set; }

        [ForeignKey("BusinessTo")]
        public Guid BusinessTo_Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public virtual User UserFrom { get; set; }
        public virtual User UserTo { get; set; }
        public virtual Business BusinessFrom { get; set; }
        public virtual Business BusinessTo { get; set; }
    }
}