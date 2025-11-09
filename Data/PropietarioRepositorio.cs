using InmobiliariaAdo.Models;
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class PropietarioRepositorio
    {
        private readonly string _connString;
        public PropietarioRepositorio(IConfiguration config)
{
    _connString = config.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Default en appsettings.json.");
}


        public async Task<List<Propietario>> ListarAsync()
        {
            var lista = new List<Propietario>();
            const string sql = @"SELECT Id, DNI, Nombre, Apellido, Domicilio, Telefono, Email
                     FROM Propietarios ORDER BY Apellido, Nombre;";


            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                var p = new Propietario
                {
                    Id = dr.GetInt32("Id"),
                    DNI = dr.GetString("DNI"),
                    Nombre = dr.GetString("Nombre"),
                    Apellido = dr.GetString("Apellido"),
                    Domicilio = dr.IsDBNull(dr.GetOrdinal("Domicilio")) ? null : dr.GetString("Domicilio"),
                    Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString("Telefono"),
                    Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? null : dr.GetString("Email"),
                };
                lista.Add(p);
            }
            return lista;
        }

        public async Task<int> CrearAsync(Propietario p)
        {
            const string sql = @"INSERT INTO Propietarios (DNI, Nombre, Apellido, Domicilio, Telefono, Email)
                     VALUES (@dni, @nom, @ape, @dom, @tel, @eml);";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dni", p.DNI);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@ape", p.Apellido);
            cmd.Parameters.AddWithValue("@dom", (object?)p.Domicilio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@eml", (object?)p.Email ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            // MySqlConnector expone LastInsertedId en el comando:
            return (int)cmd.LastInsertedId; // devuelve el nuevo Id
        }
 
                            
                             

public async Task<bool> ActualizarAsync(Propietario p)
        {
            const string sql = @"
        UPDATE Propietarios
        SET DNI = @dni,
            Nombre = @nom,
            Apellido = @ape,
            Domicilio = @dom,
            Telefono = @tel,
            Email = @eml
        WHERE Id = @id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@dni", p.DNI);
            cmd.Parameters.AddWithValue("@nom", p.Nombre);
            cmd.Parameters.AddWithValue("@ape", p.Apellido);
            cmd.Parameters.AddWithValue("@dom", (object?)p.Domicilio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tel", (object?)p.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@eml", (object?)p.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", p.Id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }

public async Task<Propietario?> ObtenerPorIdAsync(int id)
{
    const string sql = @"
        SELECT Id, DNI, Nombre, Apellido, Domicilio, Telefono, Email
        FROM Propietarios
        WHERE Id = @id
        LIMIT 1;";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", id);

    await using var dr = await cmd.ExecuteReaderAsync();
    if (!await dr.ReadAsync()) return null;

    return new Propietario
    {
       Id = dr.GetInt32("Id"),
    
    // 💡 APLICAR VERIFICACIÓN ISDBNULL AQUÍ:
    DNI = dr.IsDBNull(dr.GetOrdinal("DNI")) ? null : dr.GetString("DNI"), 
    
    Nombre    = dr.GetString("Nombre"), // Asumiendo NOT NULL
    Apellido  = dr.GetString("Apellido"), // Asumiendo NOT NULL
    Domicilio = dr.IsDBNull(dr.GetOrdinal("Domicilio")) ? null : dr.GetString("Domicilio"),
    Telefono  = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString("Telefono"),
    Email     = dr.IsDBNull(dr.GetOrdinal("Email")) ? null : dr.GetString("Email"),
    };
}



public async Task<bool> EliminarAsync(int id)
{
    const string sql = @"DELETE FROM Propietarios WHERE Id=@id;";
    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", id);
    var rows = await cmd.ExecuteNonQueryAsync();
    return rows == 1;
}

public async Task<Propietario?> ObtenerPorEmailAsync(string email)
{
    const string sql = @"SELECT Id, DNI, Nombre, Apellido, Domicilio, Telefono, Email
                         FROM Propietarios
                         WHERE Email = @e;";
    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@e", email);

    await using var dr = await cmd.ExecuteReaderAsync();
    if (await dr.ReadAsync())
    {
        return new Propietario
        {
            Id        = dr.GetInt32("Id"),
            DNI       = dr.GetString("DNI"),
            Nombre    = dr.GetString("Nombre"),
            Apellido  = dr.GetString("Apellido"),
            Domicilio = dr.IsDBNull(dr.GetOrdinal("Domicilio")) ? null : dr.GetString("Domicilio"),
            Telefono  = dr.IsDBNull(dr.GetOrdinal("Telefono"))  ? null : dr.GetString("Telefono"),
            Email     = dr.IsDBNull(dr.GetOrdinal("Email"))     ? null : dr.GetString("Email"),
        };
    }
    return null;
}




    }
}
