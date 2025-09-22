using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAdo.Models
{
    public class TipoInmueble
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo es obligatorio")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        [Display(Name = "Tipo de Inmueble")]
        public string Nombre { get; set; } = "";
    }
}
