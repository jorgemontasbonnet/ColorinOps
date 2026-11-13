using System;

namespace Mantenimiento.Models;

public class Parametro
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? ModificadoPor { get; set; }
    public DateTime? ModificadoEn { get; set; }
}
