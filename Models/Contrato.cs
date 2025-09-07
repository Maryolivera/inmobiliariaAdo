using System;
using System.ComponentModel.DataAnnotations;

namespace InmobiliariaAdo.Models
{
    public class Contrato
    {
        public int Id { get; set; }

        // Para vistas (desde JOIN)
        public string? InmuebleNombre { get; set; }
        public string? PropietarioNombre { get; set; }
        public string? InquilinoNombre { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de inicio")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de fin")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El monto mensual es obligatorio")]
        [Range(1, 999999999, ErrorMessage = "El monto debe ser positivo")]
        [Display(Name = "Monto mensual")]
        public decimal MontoMensual { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Terminación anticipada")]
        public DateTime? TerminacionAnticipada { get; set; }

        // Opcional: si alguna vista resume el inmueble
        [Display(Name = "Inmueble")]
        public string? InmuebleResumen { get; set; }
    }
}
