using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cbsdg.Data.Helpers.Security
{
    public class TokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
