namespace Mantenimiento.Api.Models
{
    public class MarcarOrdenListaOrdenRequest
    {
        public Guid OrdenId { get; set; }
        public string Observaciones { get; set; } = string.Empty;

        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}
