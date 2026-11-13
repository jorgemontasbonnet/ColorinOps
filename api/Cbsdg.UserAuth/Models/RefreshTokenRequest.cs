namespace Cbsdg.UserAuth.Models
{
    public class RefreshTokenRequest
    {
        public Guid SessionId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
