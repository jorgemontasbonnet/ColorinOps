
namespace Mantenimiento.Models;

public class Usuario
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string[] Roles { get; set; } = [];
    public bool Activo { get; set; } = true;

    public Empresa[] Empresas { get; set; } = [];
}

public class Empresa
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime? UltimoAcceso { get; set; }
}

public class ConeccionEmpresa
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserPwd { get; set; } = string.Empty;
    public string Servidor { get; set; } = string.Empty;
    public string BaseDatos { get; set; } = string.Empty;
    public string Conexion { get; set; } = string.Empty;
    public bool Encriptada { get; set; } = false;
    public bool Trusted { get; set; } = false;
}


public class AuthSession
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool Cancelada { get; set; } = false;
    public DateTime Inicio { get; set; }
    public DateTime Expiracion { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TelemetryData { get; set; } = string.Empty;
}