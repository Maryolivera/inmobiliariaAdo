using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InmobiliariaAdo.Models;
using Microsoft.Extensions.Configuration; // <-- IMPORTANTE
using MySqlConnector;

namespace InmobiliariaAdo.Data
{
    public class ContratoRepositorio
    {
        private readonly string _connString;

        public ContratoRepositorio(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default");
        }

        // SELECT base con JOIN (texto legible para UI)
        private const string SELECT_JOIN = @"
            SELECT
              c.id,
              c.inmuebleId,
              c.inquilinoId,
              c.fechaInicio,
              c.fechaFin,
              c.montoMensual,
              c.terminacionAnticipada,
              i.direccion AS InmuebleNombre,
              CONCAT(p.apellido, ', ', p.nombre) AS PropietarioNombre,
              CONCAT(inq.apellido, ', ', inq.nombre) AS InquilinoNombre
            FROM contratos c
              JOIN inmuebles   i   ON i.id  = c.inmuebleId
              JOIN propietarios p   ON p.id  = i.PropietarioId   -- <<== P mayúscula
              JOIN inquilinos  inq  ON inq.id = c.inquilinoId
        ";

        // Helpers
        private static string? GetStringOrNull(MySqlDataReader dr, string name)
        {
            var i = dr.GetOrdinal(name);
            return dr.IsDBNull(i) ? null : dr.GetString(i);
        }

        private static Contrato Map(MySqlDataReader dr) => new()
        {
            Id = dr.GetInt32("id"),
            InmuebleId = dr.GetInt32("inmuebleId"),
            InquilinoId = dr.GetInt32("inquilinoId"),
            FechaInicio = dr.GetDateTime("fechaInicio"),
            FechaFin = dr.GetDateTime("fechaFin"),
            MontoMensual = dr.GetDecimal("montoMensual"),
            TerminacionAnticipada = dr.IsDBNull(dr.GetOrdinal("terminacionAnticipada"))
                ? (DateTime?)null : dr.GetDateTime("terminacionAnticipada"),
            InmuebleNombre = GetStringOrNull(dr, "InmuebleNombre"),
            PropietarioNombre = GetStringOrNull(dr, "PropietarioNombre"),
            InquilinoNombre = GetStringOrNull(dr, "InquilinoNombre"),
        };

        // ===== CRUD =====

        public async Task<List<Contrato>> ListarAsync()
        {
            var lista = new List<Contrato>();
            var sql = SELECT_JOIN + " ORDER BY c.id DESC;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
                lista.Add(Map(dr));
            return lista;
        }

        public async Task<int> CrearAsync(Contrato x)
        {
            const string sql = @"
                INSERT INTO contratos
                  (inmuebleId, inquilinoId, fechaInicio, fechaFin, montoMensual, terminacionAnticipada)
                VALUES
                  (@inm, @inq, @ini, @fin, @monto, @term);";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inm", x.InmuebleId);
            cmd.Parameters.AddWithValue("@inq", x.InquilinoId);
            cmd.Parameters.AddWithValue("@ini", x.FechaInicio);
            cmd.Parameters.AddWithValue("@fin", x.FechaFin);
            cmd.Parameters.AddWithValue("@monto", x.MontoMensual);
            cmd.Parameters.AddWithValue("@term", (object?)x.TerminacionAnticipada ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task<Contrato?> ObtenerPorIdAsync(int id)
        {
            var sql = SELECT_JOIN + " WHERE c.id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var dr = await cmd.ExecuteReaderAsync();
            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task<bool> ActualizarAsync(Contrato x)
        {
            const string sql = @"
                UPDATE contratos
                SET inmuebleId=@inm,
                    inquilinoId=@inq,
                    fechaInicio=@ini,
                    fechaFin=@fin,
                    montoMensual=@monto,
                    terminacionAnticipada=@term
                WHERE id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inm", x.InmuebleId);
            cmd.Parameters.AddWithValue("@inq", x.InquilinoId);
            cmd.Parameters.AddWithValue("@ini", x.FechaInicio);
            cmd.Parameters.AddWithValue("@fin", x.FechaFin);
            cmd.Parameters.AddWithValue("@monto", x.MontoMensual);
            cmd.Parameters.AddWithValue("@term", (object?)x.TerminacionAnticipada ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", x.Id);

            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"DELETE FROM contratos WHERE id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        // ===== EXTRAS =====

        public async Task<bool> ExisteSolapeAsync(int inmuebleId, DateTime ini, DateTime fin, int? excluirId = null)
        {
            const string sql = @"
                SELECT 1
                FROM contratos
                WHERE inmuebleId=@inm
                  AND fechaInicio < @fin
                  AND @ini < fechaFin
                  AND (@ex IS NULL OR id <> @ex)
                LIMIT 1;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inm", inmuebleId);
            cmd.Parameters.AddWithValue("@ini", ini);
            cmd.Parameters.AddWithValue("@fin", fin);
            cmd.Parameters.AddWithValue("@ex", (object?)excluirId ?? DBNull.Value);

            var obj = await cmd.ExecuteScalarAsync();
            return obj != null;
        }

        public async Task<bool> TerminarAnticipadoAsync(int idContrato, DateTime fecha)
        {
            const string sql = @"
                UPDATE contratos
                SET terminacionAnticipada=@f, fechaFin=@f
                WHERE id=@id;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@f", fecha);
            cmd.Parameters.AddWithValue("@id", idContrato);
            return await cmd.ExecuteNonQueryAsync() == 1;
        }

        public async Task<int> RenovarAsync(int idOrigen, DateTime nuevoIni, DateTime nuevoFin, decimal nuevoMonto)
        {
            const string sqlGet = @"SELECT inmuebleId, inquilinoId FROM contratos WHERE id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();

            int? inmuebleId = null;
            int? inquilinoId = null;

            await using (var getCmd = new MySqlCommand(sqlGet, conn))
            {
                getCmd.Parameters.AddWithValue("@id", idOrigen);
                await using var dr = await getCmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    inmuebleId = dr.GetInt32("inmuebleId");
                    inquilinoId = dr.GetInt32("inquilinoId");
                }
            }

            if (inmuebleId == null || inquilinoId == null) return 0;

            const string sqlIns = @"
                INSERT INTO contratos (inmuebleId, inquilinoId, fechaInicio, fechaFin, montoMensual)
                VALUES (@inm, @inq, @ini, @fin, @monto);";

            await using var insCmd = new MySqlCommand(sqlIns, conn);
            insCmd.Parameters.AddWithValue("@inm", inmuebleId);
            insCmd.Parameters.AddWithValue("@inq", inquilinoId);
            insCmd.Parameters.AddWithValue("@ini", nuevoIni);
            insCmd.Parameters.AddWithValue("@fin", nuevoFin);
            insCmd.Parameters.AddWithValue("@monto", nuevoMonto);

            await insCmd.ExecuteNonQueryAsync();
            return (int)insCmd.LastInsertedId;
        }

        public async Task<List<Contrato>> ListarVigentesAsync(DateTime hoy)
        {
            var lista = new List<Contrato>();
            var sql = SELECT_JOIN + @"
                WHERE @hoy BETWEEN c.fechaInicio AND c.fechaFin
                ORDER BY c.fechaFin;";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@hoy", hoy);
            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync()) lista.Add(Map(dr));
            return lista;
        }
        public async Task<List<Contrato>> ListarQueTerminanEnAsync(int dias)
        {
            var lista = new List<Contrato>();
            const string sql = @"
        SELECT c.id, c.inmuebleId, c.inquilinoId, c.fechaInicio, c.fechaFin, c.montoMensual, c.terminacionAnticipada,
               i.direccion            AS InmuebleNombre,
               CONCAT(p.apellido, ', ', p.nombre) AS PropietarioNombre,
               CONCAT(q.apellido, ', ', q.nombre) AS InquilinoNombre
        FROM contratos c
        JOIN inmuebles   i ON i.id = c.inmuebleId
        JOIN propietarios p ON p.id = i.propietarioId
        JOIN inquilinos   q ON q.id = c.inquilinoId
        WHERE DATEDIFF(COALESCE(c.terminacionAnticipada, c.fechaFin), CURDATE()) BETWEEN 0 AND @dias
        ORDER BY COALESCE(c.terminacionAnticipada, c.fechaFin);";

            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dias", dias);
            await using var dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                lista.Add(new Contrato
                {
                    Id = dr.GetInt32("id"),
                    InmuebleId = dr.GetInt32("inmuebleId"),
                    InquilinoId = dr.GetInt32("inquilinoId"),
                    FechaInicio = dr.GetDateTime("fechaInicio"),
                    FechaFin = dr.GetDateTime("fechaFin"),
                    MontoMensual = dr.GetDecimal("montoMensual"),
                    TerminacionAnticipada = dr.IsDBNull(dr.GetOrdinal("terminacionAnticipada")) ? null : dr.GetDateTime("terminacionAnticipada"),
                    InmuebleNombre = dr.GetString("InmuebleNombre"),
                    PropietarioNombre = dr.GetString("PropietarioNombre"),
                    InquilinoNombre = dr.GetString("InquilinoNombre"),
                });
            }
            return lista;
        }

        public async Task<List<Contrato>> ListarQueTerminanEnRangoAsync(int minDias, int maxDias)
{
    var lista = new List<Contrato>();
    const string sql = @"
        SELECT c.id, c.inmuebleId, c.inquilinoId, c.fechaInicio, c.fechaFin, c.montoMensual, c.terminacionAnticipada,
               i.direccion AS InmuebleNombre,
               CONCAT(p.apellido, ', ', p.nombre) AS PropietarioNombre,
               CONCAT(q.apellido, ', ', q.nombre) AS InquilinoNombre
        FROM contratos c
        JOIN inmuebles   i ON i.id = c.inmuebleId
        JOIN propietarios p ON p.id = i.propietarioId
        JOIN inquilinos   q ON q.id = c.inquilinoId
        WHERE DATEDIFF(COALESCE(c.terminacionAnticipada, c.fechaFin), CURDATE()) BETWEEN @min AND @max
        ORDER BY COALESCE(c.terminacionAnticipada, c.fechaFin);";

    await using var conn = new MySqlConnection(_connString);
    await conn.OpenAsync();
    await using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@min", minDias);
    cmd.Parameters.AddWithValue("@max", maxDias);
    await using var dr = await cmd.ExecuteReaderAsync();

    while (await dr.ReadAsync())
    {
        lista.Add(new Contrato
        {
            Id = dr.GetInt32("id"),
            InmuebleId = dr.GetInt32("inmuebleId"),
            InquilinoId = dr.GetInt32("inquilinoId"),
            FechaInicio = dr.GetDateTime("fechaInicio"),
            FechaFin = dr.GetDateTime("fechaFin"),
            MontoMensual = dr.GetDecimal("montoMensual"),
            TerminacionAnticipada = dr.IsDBNull(dr.GetOrdinal("terminacionAnticipada")) ? null : dr.GetDateTime("terminacionAnticipada"),
            InmuebleNombre = dr.GetString("InmuebleNombre"),
            PropietarioNombre = dr.GetString("PropietarioNombre"),
            InquilinoNombre = dr.GetString("InquilinoNombre"),
        });
    }
    return lista;
}




    }
}

