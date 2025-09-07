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
    }
}
