using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class PagoRepositorio
    {
        private readonly string _connString;
        public PagoRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        // ================== CRUD ==================

        // Listar pagos de un contrato
        public async Task<List<Pago>> ListarPorContratoAsync(int contratoId)
        {
            var lista = new List<Pago>();
            const string sql = @"SELECT Id, ContratoId, Numero, FechaPago, Detalle, Importe, Anulado
                                 FROM Pagos
                                 WHERE ContratoId=@c
                                 ORDER BY Numero;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@c", contratoId);
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Pago
                {
                    Id = dr.GetInt32("Id"),
                    ContratoId = dr.GetInt32("ContratoId"),
                    Numero = dr.GetInt32("Numero"),
                    FechaPago = dr.GetDateTime("FechaPago"),
                    Detalle = dr.GetString("Detalle"),
                    Importe = dr.GetDecimal("Importe"),
                    Anulado = dr.GetBoolean("Anulado"),
                });
            }
            return lista;
        }
        
        // Devuelve datos para el mensaje: ContratoId, Numero de pago, y nombre del Inquilino
public async Task<(int ContratoId, int Numero, string InquilinoNombre)> ObtenerResumenPagoAsync(int pagoId)
{
    const string sql = @"
        SELECT  p.ContratoId,
                p.Numero,
                CONCAT(i.Apellido, ' ', i.Nombre) AS InquilinoNombre
        FROM Pagos p
        JOIN Contratos c   ON p.ContratoId = c.Id
        JOIN Inquilinos i  ON c.InquilinoId = i.Id
        WHERE p.Id = @id;";

    await using var conn = new MySqlConnector.MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlConnector.MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", pagoId);

    await using var dr = await cmd.ExecuteReaderAsync();
    if (await dr.ReadAsync())
    {
        var contratoId = dr.GetInt32("ContratoId");
        var numero = dr.GetInt32("Numero");
        var nombre = dr.GetString("InquilinoNombre");
        return (contratoId, numero, nombre);
    }
    return (0, 0, "");
}


// Devuelve el próximo número de pago dentro de un contrato
        public async Task<int> ObtenerSiguienteNumeroAsync(int contratoId)
        {
            const string sql = "SELECT COALESCE(MAX(Numero),0)+1 FROM Pagos WHERE ContratoId=@c;";
            await using var conn = new MySqlConnector.MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlConnector.MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@c", contratoId);
            var obj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(obj);
        }



        // Crear pago
        public async Task<int> CrearAsync(Pago x)
        {
            const string sql = @"INSERT INTO Pagos
                                (ContratoId, Numero, FechaPago, Detalle, Importe, Anulado)
                                 VALUES (@c, @n, @f, @d, @i, 0);";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@c", x.ContratoId);
            cmd.Parameters.AddWithValue("@n", x.Numero);
            cmd.Parameters.AddWithValue("@f", x.FechaPago);
            cmd.Parameters.AddWithValue("@d", x.Detalle);
            cmd.Parameters.AddWithValue("@i", x.Importe);

            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        // Obtener pago por Id
        public async Task<Pago?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"SELECT Id, ContratoId, Numero, FechaPago, Detalle, Importe, Anulado
                                 FROM Pagos WHERE Id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                return new Pago
                {
                    Id = dr.GetInt32("Id"),
                    ContratoId = dr.GetInt32("ContratoId"),
                    Numero = dr.GetInt32("Numero"),
                    FechaPago = dr.GetDateTime("FechaPago"),
                    Detalle = dr.GetString("Detalle"),
                    Importe = dr.GetDecimal("Importe"),
                    Anulado = dr.GetBoolean("Anulado"),
                };
            }
            return null;
        }

        // Anular pago (update)
        public async Task<bool> AnularAsync(int id)
        {
            const string sql = @"UPDATE Pagos SET Anulado=1 WHERE Id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }

        // Para redirigir después de anular: obtener contrato asociado
        public async Task<int> ObtenerContratoIdAsync(int pagoId)
        {
            const string sql = @"SELECT ContratoId FROM Pagos WHERE Id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", pagoId);

            var obj = await cmd.ExecuteScalarAsync();
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        // Editar solo el concepto (Detalle) de un pago
        public async Task<bool> EditarConceptoAsync(int id, string detalle)
        {
            const string sql = @"UPDATE Pagos SET Detalle=@d WHERE Id=@id AND Anulado=0;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@d", detalle);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
       // Devuelve "Apellido Nombre" del inquilino del contrato
public async Task<string> ObtenerInquilinoNombrePorContratoAsync(int contratoId)
{
    const string sql = @"
        SELECT CONCAT(i.Apellido, ' ', i.Nombre) AS InquilinoNombre
        FROM Contratos c
        JOIN Inquilinos i ON c.InquilinoId = i.Id
        WHERE c.Id = @id;
    ";

    await using var conn = new MySqlConnector.MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlConnector.MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", contratoId);

    var obj = await cmd.ExecuteScalarAsync();
    return obj == null ? "" : Convert.ToString(obj)!;
}





    }
}
