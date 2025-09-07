using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAdo.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "Debe especificar el uso")]
        [StringLength(20)]
        [Display(Name = "Uso")] // Residencial / Comercial
        public string Uso { get; set; } = "Residencial";

        [Required(ErrorMessage = "Debe especificar el tipo de inmueble")]
        [StringLength(50)]
        [Display(Name = "Tipo")] // casa, departamento, local, depósito...
        public string Tipo { get; set; }

        [Range(1, 99, ErrorMessage = "Los ambientes deben ser entre 1 y 99")]
        public int Ambientes { get; set; }

        [Range(0, 99999, ErrorMessage = "La superficie no puede ser negativa")]
        [Display(Name = "Superficie (m²)")]
        public int Superficie { get; set; }

        [Range(0, 999999999, ErrorMessage = "El precio debe ser positivo")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [StringLength(100)]
        public string? Coordenadas { get; set; }

        [StringLength(200)]
        [Display(Name = "Portada (imagen)")]
        public string? Portada { get; set; }  // ruta de imagen o "Sin foto"

        [Display(Name = "Suspendido")]
        public bool Suspendido { get; set; } = false;

        // --- Propiedades auxiliares para vistas ---
        [Display(Name = "Dueño")]
        public string? PropietarioNombre { get; set; }
    }
}
