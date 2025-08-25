using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class InquilinoRepositorio
    {
        private readonly string _connString;
        public InquilinoRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        public async Task<List<Inquilino>> ListarAsync()
        {
            var lista = new List<Inquilino>();
            const string sql = @"SELECT Id, DNI, Nombre, Apellido, Telefono, Email
                                 FROM Inquilinos
                                 ORDER BY Apellido, Nombre;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Inquilino
                {
                    Id = dr.GetInt32("Id"),
                    DNI = dr.GetString("DNI"),
                    Nombre = dr.GetString("Nombre"),
                    Apellido = dr.GetString("Apellido"),
                    Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString("Telefono"),
                    Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? null : dr.GetString("Email"),
                });
            }
            return lista;
        }

        public async Task<int> CrearAsync(Inquilino x)
        {
            const string sql = @"INSERT INTO Inquilinos (DNI, Nombre, Apellido, Telefono, Email)
                                 VALUES (@dni, @nom, @ape, @tel, @eml);";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dni", x.DNI);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            cmd.Parameters.AddWithValue("@ape", x.Apellido);
            cmd.Parameters.AddWithValue("@tel", (object?)x.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@eml", (object?)x.Email ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task<Inquilino?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"SELECT Id, DNI, Nombre, Apellido, Telefono, Email
                                 FROM Inquilinos WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                return new Inquilino
                {
                    Id = dr.GetInt32("Id"),
                    DNI = dr.GetString("DNI"),
                    Nombre = dr.GetString("Nombre"),
                    Apellido = dr.GetString("Apellido"),
                    Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString("Telefono"),
                    Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? null : dr.GetString("Email"),
                };
            }
            return null;
        }

        public async Task<bool> ActualizarAsync(Inquilino x)
        {
            const string sql = @"UPDATE Inquilinos
                                 SET DNI=@dni, Nombre=@nom, Apellido=@ape, Telefono=@tel, Email=@eml
                                 WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dni", x.DNI);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            cmd.Parameters.AddWithValue("@ape", x.Apellido);
            cmd.Parameters.AddWithValue("@tel", (object?)x.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@eml", (object?)x.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", x.Id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"DELETE FROM Inquilinos WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
    }
}
