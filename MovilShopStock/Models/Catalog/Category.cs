using MovilShopStock.Models.Handlers;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MovilShopStock.Models.Catalog
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [DisplayName("Nombre")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Acción en Entrada")]
        public ActionConstants ActionIn { get; set; }

        [DisplayName("Acción en Salida")]
        public ActionConstants ActionOut { get; set; }

        [DisplayName("Acción en Resumen")]
        public ActionConstants SystemAction { get; set; }

        public DateTime LastUpdated { get; set; }

        [DisplayName("Mostrar en Resumen")]
        public bool ShowDashboard { get; set; }
    }
}