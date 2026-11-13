using Mantenimiento.Models;

namespace Cbsdg.UserAuth.Models
{
    public class LoginResponse
    {
        public Guid SessionId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public Empresa[] Tenants { get; set; } = [];
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}