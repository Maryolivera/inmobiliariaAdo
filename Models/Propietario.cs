using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAdo.Models
{
    public class Propietario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(20)]
        public string DNI { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100)]
        public string Apellido { get; set; }

        [StringLength(50)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Domicilio { get; set; }

    }
}
