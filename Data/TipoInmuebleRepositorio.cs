using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class TipoInmuebleRepositorio
    {
        private readonly string _connString;

        public TipoInmuebleRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        public async Task<List<TipoInmueble>> ListarAsync()
        {
            var lista = new List<TipoInmueble>();
            const string sql = @"SELECT Id, Nombre FROM TiposInmueble ORDER BY Nombre;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new TipoInmueble
                {
                    Id = dr.GetInt32("Id"),
                    Nombre = dr.GetString("Nombre")
                });
            }
            return lista;
        }

        public async Task<TipoInmueble?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"SELECT Id, Nombre FROM TiposInmueble WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                return new TipoInmueble
                {
                    Id = dr.GetInt32("Id"),
                    Nombre = dr.GetString("Nombre")
                };
            }
            return null;
        }

        public async Task<int> CrearAsync(TipoInmueble x)
        {
            const string sql = @"INSERT INTO TiposInmueble (Nombre) VALUES (@nom);";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task<bool> ActualizarAsync(TipoInmueble x)
        {
            const string sql = @"UPDATE TiposInmueble SET Nombre=@nom WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            cmd.Parameters.AddWithValue("@id", x.Id);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"DELETE FROM TiposInmueble WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }
    }
}

