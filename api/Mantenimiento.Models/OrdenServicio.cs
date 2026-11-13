using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Mantenimiento.Models;

public class OrdenServicio
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int StatusId { get; set; }
    public string StatusNombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }
    
    public string Servicio { get; set; } = string.Empty;
    public string RealizadoPor { get; set; } = string.Empty;
    
    public string? InspeccionadoPor { get; set; }

    public DateTime? Inicio { get; set; }
    public DateTime? Fin { get; set; }
    public string Usuario { get; set; } = string.Empty;

    public string Referencia { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public string Ejecutor { get; set; } = string.Empty;

    public string Observacion { get; set; } = string.Empty;
    public string TipoOrden { get; set; } = string.Empty;

    public bool Pospuesto { get; set; }
    
    public string Brigada { get; set; } = string.Empty;

    public decimal Combustible { get; set; } = 0;
    public decimal Peaje { get; set; } = 0;

    public string Supervisor { get; set; } = string.Empty;
    
    public string Encargado { get; set; } = string.Empty;
    public decimal Kilometros { get; set; } = 0;
    public bool Aprobado { get; set; }
    public string AprobadoPor { get; set; } = string.Empty;
    public bool Cancelado { get; set; }
    public bool Cerrado { get; set; }
    public bool Listo { get; set; }
    public bool Inspeccion { get; set; }

    public List<OrdenServicioTrabajo> Trabajos { get; set; } = [];

    public List<Foto> Fotos { get; set; } = [];
}

public class OrdenServicioTrabajo
{
    public Guid Id { get; set; }
    public Guid OrdenServicioId { get; set; }
    public string TipoTrabajoId { get; set; } = string.Empty;
    public string TipoTrabajoNombre { get; set; } = string.Empty;

    public string Trabajo { get; set; } = string.Empty;
    public int Orden { get; set; } = 1;
    public string EspacioId { get; set; } = string.Empty;
    public string EspacioNombre { get; set; } = string.Empty;
    public decimal Costo { get; set; } = 0;

    public Guid ImpresoId { get; set; }
    public string ImpresoCodigo { get; set; } = string.Empty;
    public string ImpresoNombre { get; set; } = string.Empty;
  
    public Guid ContratoId { get; set; }
    public string BrigadaId { get; set; } = string.Empty;
    
    public DateTime FechaRegistro { get; set; }
    public DateTime? FechaObjetivo { get; set; }

    public string Referencia { get; set; } = string.Empty;

    public bool Cancelado { get; set; }
    public int ServicioId { get; set; } 
    public string Notas { get; set; } = string.Empty;
    
    public bool Iluminacion { get; set; }
    public bool Recibido { get; set; }
    
    public DateTime? RecibidoFecha { get; set; }
    public string RecibidoUsuario { get; set; } = string.Empty;
    public string Dimensiones { get; set; } = string.Empty;
    
    public string ImpresoManual { get; set; } = string.Empty;

    public List<FotoEspacio> Fotos { get; set; } = [];
}

public class  Foto
{
    public Guid Id { get; set; }
    public Guid OrdenServicioId { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public byte[] Imagen { get; set; } = [];
}