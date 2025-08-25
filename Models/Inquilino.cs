using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAdo.Models
{
    public class Inquilino
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string DNI { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }
    }
}
