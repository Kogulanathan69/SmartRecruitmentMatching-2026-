using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexHire.Application.DTOs.Auth;

public sealed class ResendEmailOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
}