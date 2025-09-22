using System.Security.Cryptography;
using System.Text;
using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class UsuarioRepositorio
    {
        private readonly string _connString;
        private readonly string _salt;

        public UsuarioRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json.");
            _salt = config["Auth:Salt"]
                ?? throw new InvalidOperationException("Falta Auth:Salt en appsettings.json.");
        }

        private string Hash(string textoPlano)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_salt));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        // Buscar usuario por email (para login)
        public async Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            const string sql = @"SELECT Id, Email, ClaveHash, Rol, Apellido, Nombre, Avatar, Activo
                                 FROM Usuarios WHERE Email=@e AND Activo=1;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@e", email);
            await using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                return new Usuario
                {
                    Id = dr.GetInt32("Id"),
                    Email = dr.GetString("Email"),
                    ClaveHash = dr.GetString("ClaveHash"),
                    Rol = dr.GetString("Rol"),
                    Apellido = dr.GetString("Apellido"),
                    Nombre = dr.GetString("Nombre"),
                    Avatar = dr.IsDBNull(dr.GetOrdinal("Avatar")) ? null : dr.GetString("Avatar"),
                    Activo = dr.GetBoolean("Activo"),
                };
            }
            return null;
        }

        // Validar login (compara hash)
        public async Task<Usuario?> ValidarLoginAsync(string email, string clavePlano)
        {
            var u = await BuscarPorEmailAsync(email);
            if (u == null) return null;
            var hash = Hash(clavePlano);
            return hash == u.ClaveHash ? u : null;
        }

        // Crear nuevo usuario
        public async Task<int> CrearAsync(Usuario u, string clavePlano)
        {
            const string sql = @"INSERT INTO Usuarios (Email, ClaveHash, Rol, Apellido, Nombre, Avatar, Activo)
                                 VALUES (@e, @h, @r, @a, @n, @v, 1);
                                 SELECT LAST_INSERT_ID();";
            var hash = Hash(clavePlano);
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@e", u.Email);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@r", u.Rol);
            cmd.Parameters.AddWithValue("@a", u.Apellido);
            cmd.Parameters.AddWithValue("@n", u.Nombre);
            cmd.Parameters.AddWithValue("@v", (object?)u.Avatar ?? DBNull.Value);
            var obj = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(obj);
        }

        // Cambiar clave
        public async Task<bool> CambiarClaveAsync(int id, string nuevaClavePlano)
        {
            const string sql = @"UPDATE Usuarios SET ClaveHash=@h WHERE Id=@id;";
            var hash = Hash(nuevaClavePlano);
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        // Actualizar perfil (Empleado/Admin)
        public async Task<bool> ActualizarPerfilAsync(Usuario u)
        {
            const string sql = @"UPDATE Usuarios
                                 SET Apellido=@a, Nombre=@n, Avatar=@v
                                 WHERE Id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@a", u.Apellido);
            cmd.Parameters.AddWithValue("@n", u.Nombre);
            cmd.Parameters.AddWithValue("@v", (object?)u.Avatar ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", u.Id);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        // ========== MÉTODOS PARA ABM DE ADMINISTRADOR ==========

        // Listar usuarios
        public async Task<List<Usuario>> ListarAsync(bool incluirInactivos = false)
        {
            var lista = new List<Usuario>();
            var where = incluirInactivos ? "" : "WHERE u.Activo = 1";

            string sql = $@"
                SELECT u.Id, u.Email, u.ClaveHash, u.Rol, u.Apellido, u.Nombre, u.Avatar, u.Activo
                FROM Usuarios u
                {where}
                ORDER BY u.Apellido, u.Nombre;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    Id = dr.GetInt32("Id"),
                    Email = dr.GetString("Email"),
                    ClaveHash = dr.GetString("ClaveHash"),
                    Rol = dr.GetString("Rol"),
                    Apellido = dr.GetString("Apellido"),
                    Nombre = dr.GetString("Nombre"),
                    Avatar = dr.IsDBNull(dr.GetOrdinal("Avatar")) ? null : dr.GetString("Avatar"),
                    Activo = dr.GetBoolean("Activo")
                });
            }
            return lista;
        }

        // Buscar por Id
        public async Task<Usuario?> BuscarPorIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Email, ClaveHash, Rol, Apellido, Nombre, Avatar, Activo
                FROM Usuarios
                WHERE Id = @id
                LIMIT 1;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new Usuario
            {
                Id = dr.GetInt32("Id"),
                Email = dr.GetString("Email"),
                ClaveHash = dr.GetString("ClaveHash"),
                Rol = dr.GetString("Rol"),
                Apellido = dr.GetString("Apellido"),
                Nombre = dr.GetString("Nombre"),
                Avatar = dr.IsDBNull(dr.GetOrdinal("Avatar")) ? null : dr.GetString("Avatar"),
                Activo = dr.GetBoolean("Activo")
            };
        }

        // Actualizar usuario (solo Admin)
        public async Task<bool> ActualizarAsync(Usuario u)
        {
            const string sql = @"
                UPDATE Usuarios
                SET Email = @e,
                    Rol = @r,
                    Apellido = @a,
                    Nombre = @n,
                    Avatar = @v,
                    Activo = @act
                WHERE Id = @id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@e", u.Email);
            cmd.Parameters.AddWithValue("@r", u.Rol);
            cmd.Parameters.AddWithValue("@a", u.Apellido);
            cmd.Parameters.AddWithValue("@n", u.Nombre);
            cmd.Parameters.AddWithValue("@v", (object?)u.Avatar ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@act", u.Activo);
            cmd.Parameters.AddWithValue("@id", u.Id);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        // Eliminar usuario (borrado lógico)
        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"UPDATE Usuarios SET Activo = 0 WHERE Id = @id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }
    }
}

