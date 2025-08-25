using MySqlConnector;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaAdo.Data
{
    public class Db
    {
        private readonly string _connString;
        public Db(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                using var conn = new MySqlConnection(_connString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand("SELECT 1", conn);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
            catch
            {
                return false; // si falla, devolvemos false (luego vemos el error en consola)
            }
        }
    }
}
