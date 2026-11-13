using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cbsdg.UserAuth.Models;

public class ChgPwdRequest
{
    public string Username { get; set; } = string.Empty;
    public string OldPwd { get; set; } = string.Empty;
    public string NewPwd { get; set; } = string.Empty;
}
