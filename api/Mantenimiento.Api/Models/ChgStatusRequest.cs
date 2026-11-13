using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mantenimiento.Api.Models;

public class CambioStatusRequest
{
    public Guid Id { get; set; }
    public string NuevoStatusId { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}
