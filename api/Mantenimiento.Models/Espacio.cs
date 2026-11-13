using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento.Models
{
    public class FotoEspacio
    {
        public string EspacioId { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;        
        public DateTime FechaRegistro { get; set; }
        public DateTime Fecha { get; set; }
        public int NumImagen { get; set; }

        public byte[] Imagen { get; set; } = [];
    }

    public class Impreso
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Campanna { get; set; } = string.Empty;
        public byte[] Imagen { get; set; } = [];
    }
}
