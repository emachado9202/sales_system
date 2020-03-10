using MovilShopStock.Models.Catalog;
using System.ComponentModel.DataAnnotations;

namespace MovilShopStock.Models.View
{
    public class BusinessInvitationModel
    {
        public BusinessModel Business { get; set; }
        public User Owner { get; set; }

        public bool AlreadySubscribed { get; set; }
        public bool AlreadySystem { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "La {0} necesita tener al menos {2} caracteres de longitud.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string BusinessId { get; set; }
        public string OwnerId { get; set; }
    }
}