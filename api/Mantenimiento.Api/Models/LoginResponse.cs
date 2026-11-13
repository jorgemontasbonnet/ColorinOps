using Mantenimiento.Models;

namespace Mantenimiento.Api.Models
{
    public class LoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public Empresa[] Tenants { get; set; } = [];
        public string Token { get; set; } = string.Empty;
    }
}