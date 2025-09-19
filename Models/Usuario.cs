namespace InmobiliariaAdo.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string ClaveHash { get; set; } = "";
        public string Rol { get; set; } = "Empleado"; // Admin | Empleado
        public string Apellido { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Avatar { get; set; }
        public bool Activo { get; set; } = true;

        // Conveniencia para mostrar
        public string NombreCompleto => $"{Apellido} {Nombre}";
    }
}
