namespace InmobiliariaAdo.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public int ContratoId { get; set; }
        public int Numero { get; set; }             // número de pago (1, 2, 3…)
        public DateTime FechaPago { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public decimal Importe { get; set; }
        public bool Anulado { get; set; }           // true = anulado
    }
}
