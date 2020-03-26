using System.ComponentModel.DataAnnotations;

namespace MovilShopStock.Models.Catalog
{
    public class ActivityLogType
    {
        [Key]
        public int Id { get; set; }

        public string SystemKeybord { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}