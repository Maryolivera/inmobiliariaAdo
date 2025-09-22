using System.Text.Json;

namespace InmobiliariaAdo.Services
{
    public static class AuditoriaArchivo
    {
        private static readonly string Carpeta =
            Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "auditoria");

        private static readonly string ArchivoPagos =
            Path.Combine(Carpeta, "pagos.jsonl");

        private static readonly string ArchivoContratos =
            Path.Combine(Carpeta, "contratos.jsonl");

        private static void AsegurarCarpeta()
        {
            Directory.CreateDirectory(Carpeta);
        }

        // ================== PAGOS ==================
        public static void RegistrarPago(int pagoId, string accion, string usuario)
        {
            AsegurarCarpeta();
            var evt = new
            {
                Tipo = "Pago",
                PagoId = pagoId,
                Accion = accion,         // "creado" | "anulado"
                Usuario = usuario,
                Fecha = DateTime.Now
            };
            File.AppendAllText(ArchivoPagos,
                JsonSerializer.Serialize(evt) + Environment.NewLine);
        }

        public static (string? CreadoPor, DateTime? FechaCreado,
                       string? AnuladoPor, DateTime? FechaAnulado)
            ObtenerAuditoriaPago(int pagoId)
        {
            if (!File.Exists(ArchivoPagos))
                return (null, null, null, null);

            string? creadoPor = null; DateTime? fechaCreado = null;
            string? anuladoPor = null; DateTime? fechaAnulado = null;

            foreach (var linea in File.ReadAllLines(ArchivoPagos))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(linea);
                    var r = doc.RootElement;
                    if (r.GetProperty("Tipo").GetString() != "Pago") continue;
                    if (r.GetProperty("PagoId").GetInt32() != pagoId) continue;

                    var accion = r.GetProperty("Accion").GetString();
                    var usuario = r.GetProperty("Usuario").GetString();
                    var fecha = r.GetProperty("Fecha").GetDateTime();

                    if (accion == "creado" && creadoPor == null)
                    {
                        creadoPor = usuario; fechaCreado = fecha;
                    }
                    else if (accion == "anulado")
                    {
                        anuladoPor = usuario; fechaAnulado = fecha;
                    }
                }
                catch { /* línea dañada -> la ignoro */ }
            }

            return (creadoPor, fechaCreado, anuladoPor, fechaAnulado);
        }

        // ================== CONTRATOS ==================
        public static void RegistrarContrato(int contratoId, string accion, string usuario)
        {
            AsegurarCarpeta();
            var evt = new
            {
                Tipo = "Contrato",
                ContratoId = contratoId,
                Accion = accion,         // "creado" | "terminado"
                Usuario = usuario,
                Fecha = DateTime.Now
            };
            File.AppendAllText(ArchivoContratos,
                JsonSerializer.Serialize(evt) + Environment.NewLine);
        }

        public static (string? CreadoPor, DateTime? FechaCreado,
                       string? TerminadoPor, DateTime? FechaTerminado)
            ObtenerAuditoriaContrato(int contratoId)
        {
            if (!File.Exists(ArchivoContratos))
                return (null, null, null, null);

            string? creadoPor = null; DateTime? fechaCreado = null;
            string? terminadoPor = null; DateTime? fechaTerminado = null;

            foreach (var linea in File.ReadAllLines(ArchivoContratos))
            {
                if (string.IsNullOrWhiteSpace(linea)) continue;
                try
                {
                    using var doc = JsonDocument.Parse(linea);
                    var r = doc.RootElement;
                    if (r.GetProperty("Tipo").GetString() != "Contrato") continue;
                    if (r.GetProperty("ContratoId").GetInt32() != contratoId) continue;

                    var accion = r.GetProperty("Accion").GetString();
                    var usuario = r.GetProperty("Usuario").GetString();
                    var fecha = r.GetProperty("Fecha").GetDateTime();

                    if (accion == "creado" && creadoPor == null)
                    {
                        creadoPor = usuario; fechaCreado = fecha;
                    }
                    else if (accion == "terminado")
                    {
                        terminadoPor = usuario; fechaTerminado = fecha;
                    }
                }
                catch { /* línea dañada -> la ignoro */ }
            }

            return (creadoPor, fechaCreado, terminadoPor, fechaTerminado);
        }
    }
}

