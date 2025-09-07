using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class InmuebleRepositorio
    {
        private readonly string _connString;
        public InmuebleRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        // ================== CRUD BÁSICO (mismo orden que Inquilino/Contrato) ==================
        
        public async Task<List<Inmueble>> ListarAsync()
{
    var lista = new List<Inmueble>();
    const string sql = @"SELECT i.Id, i.PropietarioId, i.Direccion, i.Uso, i.Tipo, i.Ambientes, i.Superficie,
                                i.Coordenadas, i.Precio, i.Portada, i.Suspendido,
                                CONCAT(p.Apellido, ', ', p.Nombre) AS PropietarioNombre
                         FROM Inmuebles i
                         JOIN Propietarios p ON p.Id = i.PropietarioId
                         ORDER BY i.Id DESC;";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    await using var dr = await cmd.ExecuteReaderAsync();

    while (await dr.ReadAsync())
    {
        lista.Add(new Inmueble
        {
            Id = dr.GetInt32("Id"),
            PropietarioId = dr.GetInt32("PropietarioId"),
            Direccion = dr.GetString("Direccion"),
            Uso = dr.GetString("Uso"),
            Tipo = dr.GetString("Tipo"),
            Ambientes = dr.GetInt32("Ambientes"),
            Superficie = dr.GetInt32("Superficie"),
            Coordenadas = dr.IsDBNull(dr.GetOrdinal("Coordenadas")) ? null : dr.GetString("Coordenadas"),
            Precio = dr.GetDecimal("Precio"),
            Portada = dr.IsDBNull(dr.GetOrdinal("Portada")) ? null : dr.GetString("Portada"),
            Suspendido = dr.GetBoolean("Suspendido"),
            PropietarioNombre = dr.GetString("PropietarioNombre") // 🔹 acá mapeamos el nombre
        });
    }

    return lista;
}


        public async Task<int> CrearAsync(Inmueble x)
        {
            const string sql = @"INSERT INTO Inmuebles
                                (PropietarioId, Direccion, Uso, Tipo, Ambientes, Superficie, Coordenadas, Precio, Portada, Suspendido)
                                 VALUES (@prop, @dir, @uso, @tipo, @amb, @sup, @coord, @pre, @port, @susp);";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@prop", x.PropietarioId);
            cmd.Parameters.AddWithValue("@dir", x.Direccion);
            cmd.Parameters.AddWithValue("@uso", x.Uso);
            cmd.Parameters.AddWithValue("@tipo", x.Tipo);
            cmd.Parameters.AddWithValue("@amb", x.Ambientes);
            cmd.Parameters.AddWithValue("@sup", x.Superficie);
            cmd.Parameters.AddWithValue("@coord", (object?)x.Coordenadas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pre", x.Precio);
            cmd.Parameters.AddWithValue("@port", (object?)x.Portada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@susp", x.Suspendido);

            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task<Inmueble?> ObtenerPorIdAsync(int id)
{
    const string sql = @"SELECT i.Id, i.PropietarioId, i.Direccion, i.Uso, i.Tipo, i.Ambientes, i.Superficie,
                                i.Coordenadas, i.Precio, i.Portada, i.Suspendido,
                                CONCAT(p.Apellido, ', ', p.Nombre) AS PropietarioNombre
                         FROM Inmuebles i
                         JOIN Propietarios p ON p.Id = i.PropietarioId
                         WHERE i.Id=@id;";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", id);
    await using var dr = await cmd.ExecuteReaderAsync();

    if (await dr.ReadAsync())
    {
        return new Inmueble
        {
            Id = dr.GetInt32("Id"),
            PropietarioId = dr.GetInt32("PropietarioId"),
            Direccion = dr.GetString("Direccion"),
            Uso = dr.GetString("Uso"),
            Tipo = dr.GetString("Tipo"),
            Ambientes = dr.GetInt32("Ambientes"),
            Superficie = dr.GetInt32("Superficie"),
            Coordenadas = dr.IsDBNull(dr.GetOrdinal("Coordenadas")) ? null : dr.GetString("Coordenadas"),
            Precio = dr.GetDecimal("Precio"),
            Portada = dr.IsDBNull(dr.GetOrdinal("Portada")) ? null : dr.GetString("Portada"),
            Suspendido = dr.GetBoolean("Suspendido"),
            PropietarioNombre = dr.GetString("PropietarioNombre") // <-- agregado
        };
    }
    return null;
}



        public async Task<bool> ActualizarAsync(Inmueble x)
        {
            const string sql = @"UPDATE Inmuebles
                                 SET PropietarioId=@prop,
                                     Direccion=@dir,
                                     Uso=@uso,
                                     Tipo=@tipo,
                                     Ambientes=@amb,
                                     Superficie=@sup,
                                     Coordenadas=@coord,
                                     Precio=@pre,
                                     Portada=@port,
                                     Suspendido=@susp
                                 WHERE Id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@prop", x.PropietarioId);
            cmd.Parameters.AddWithValue("@dir", x.Direccion);
            cmd.Parameters.AddWithValue("@uso", x.Uso);
            cmd.Parameters.AddWithValue("@tipo", x.Tipo);
            cmd.Parameters.AddWithValue("@amb", x.Ambientes);
            cmd.Parameters.AddWithValue("@sup", x.Superficie);
            cmd.Parameters.AddWithValue("@coord", (object?)x.Coordenadas ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pre", x.Precio);
            cmd.Parameters.AddWithValue("@port", (object?)x.Portada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@susp", x.Suspendido);
            cmd.Parameters.AddWithValue("@id", x.Id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
        public async Task<List<Inmueble>> ListarDisponiblesHoyAsync()
{
    var lista = new List<Inmueble>();
    const string sql = @"
        SELECT i.*
        FROM inmuebles i
        WHERE i.suspendido = 0
          AND NOT EXISTS (
              SELECT 1
              FROM contratos c
              WHERE c.inmuebleId = i.id
                AND CURDATE() BETWEEN c.fechaInicio AND c.fechaFin
          )
        ORDER BY i.direccion;";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    await using var dr = await cmd.ExecuteReaderAsync();
    while (await dr.ReadAsync())
    {
        lista.Add(new Inmueble
        {
            Id = dr.GetInt32("id"),
            PropietarioId = dr.GetInt32("propietarioId"),
            Direccion = dr.GetString("direccion"),
            Uso = dr.GetString("uso"),
            Tipo = dr.GetString("tipo"),
            Ambientes = dr.GetInt32("ambientes"),
            Superficie = dr.GetInt32("superficie"),
            Coordenadas = dr.IsDBNull(dr.GetOrdinal("coordenadas")) ? null : dr.GetString("coordenadas"),
            Precio = dr.GetDecimal("precio"),
            Portada = dr.IsDBNull(dr.GetOrdinal("portada")) ? null : dr.GetString("portada"),
            Suspendido = dr.GetBoolean("suspendido"),
        });
    }
    return lista;
}


        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"DELETE FROM Inmuebles WHERE Id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
        public async Task<List<(int Id, string Texto)>> ListarParaComboAsync()
{
    var res = new List<(int, string)>();
    const string sql = @"
        SELECT i.id,
               CONCAT(i.direccion, ' — Prop: ', p.apellido, ', ', p.nombre) AS texto
        FROM inmuebles i
        JOIN propietarios p ON i.PropietarioId = p.id
        ORDER BY i.direccion;";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    await using var dr = await cmd.ExecuteReaderAsync();
    while (await dr.ReadAsync())
    {
        res.Add((dr.GetInt32("id"), dr.GetString("texto")));
    }
    return res;
}

    }
}
