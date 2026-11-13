namespace Mantenimiento.Api.Models
{
    public class UploadFotoRequest
    {
        public Guid OrdenId { get; set; }
        public string Observaciones { get; set; } = string.Empty;
    }

    public class UploadFotoEspacioRequest
    {
        public string EspacioId { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Observaciones { get; set; } = string.Empty;
    }
}
