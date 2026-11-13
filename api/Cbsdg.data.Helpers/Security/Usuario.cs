using System;
using System.Collections.Generic;
using System.Text;

namespace Cbsdg.Data.Helpers.Security
{
    public class Usuario
    {    
        public Guid Id { get; set; }
        public required string UserId { get; set; }
        public required string FriendlyName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string[] Roles { get; set; } = [];
        public bool Activo { get; set; } = true;

    }
}
