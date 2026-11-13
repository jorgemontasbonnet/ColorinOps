using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento.Models;

public class Permiso
{
    public bool Abrir { get; set; }
    public bool Agregar { get; set; }
    public bool Editar { get; set; }
    public bool Borrar { get; set; }
    public bool Imprimir { get; set; }
    public bool Anular { get; set; }
    public bool Aprobar { get; set; }
}
