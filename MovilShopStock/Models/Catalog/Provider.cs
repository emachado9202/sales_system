using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovilShopStock.Models.Catalog
{
    public class Provider
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Contacto (Email/Teléfono)")]
        public string Contact { get; set; }

        public DateTime LastUpdated { get; set; }

        [ForeignKey("Business")]
        public Guid Business_Id { get; set; }

        public virtual Business Business { get; set; }
    }
}