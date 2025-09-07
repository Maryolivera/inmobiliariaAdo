using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InmobiliariaAdo.Models;
using Microsoft.Extensions.Configuration;
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

        // ==== Helpers ====
        private static string? GetStringOrNull(MySqlDataReader dr, string name)
        {
            var i = dr.GetOrdinal(name);
            return dr.IsDBNull(i) ? null : dr.GetString(i);
        }

        private static Inquilino Map(MySqlDataReader dr) => new()
        {
            Id       = dr.GetInt32("id"),
            Nombre   = dr.GetString("nombre"),
            Apellido = dr.GetString("apellido"),
            DNI     = dr.GetString("dni"),
            Telefono = GetStringOrNull(dr, "telefono"),
            Email    = GetStringOrNull(dr, "email"),
           
        };

        // ==== CRUD ====

        public async Task<List<Inquilino>> ListarAsync()
        {
            var lista = new List<Inquilino>();
            const string sql = @"SELECT id, nombre, apellido, dni, telefono, email
                                 FROM inquilinos ORDER BY apellido, nombre;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
                lista.Add(Map(dr));
            return lista;
        }

        public async Task<Inquilino?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"SELECT id, nombre, apellido, dni, telefono, email
                                 FROM inquilinos WHERE id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await using var dr = await cmd.ExecuteReaderAsync();
            return await dr.ReadAsync() ? Map(dr) : null;
        }

        public async Task<int> CrearAsync(Inquilino x)
        {
            const string sql = @"
                INSERT INTO inquilinos (nombre, apellido, dni, telefono, email)
                VALUES (@nom, @ape, @dni, @tel, @ema);";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            cmd.Parameters.AddWithValue("@ape", x.Apellido);
            cmd.Parameters.AddWithValue("@dni", x.DNI);
            cmd.Parameters.AddWithValue("@tel", (object?)x.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ema", (object?)x.Email ?? DBNull.Value);
            
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task<bool> ActualizarAsync(Inquilino x)
        {
            const string sql = @"
                UPDATE inquilinos
                SET nombre=@nom, apellido=@ape, dni=@dni, telefono=@tel, email=@ema
                WHERE id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", x.Id);
            cmd.Parameters.AddWithValue("@nom", x.Nombre);
            cmd.Parameters.AddWithValue("@ape", x.Apellido);
            cmd.Parameters.AddWithValue("@dni", x.DNI);
            cmd.Parameters.AddWithValue("@tel", (object?)x.Telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ema", (object?)x.Email ?? DBNull.Value);
           
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            const string sql = @"DELETE FROM inquilinos WHERE id=@id;";
            await using var conn = new MySqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows == 1;
        }
    }
}

