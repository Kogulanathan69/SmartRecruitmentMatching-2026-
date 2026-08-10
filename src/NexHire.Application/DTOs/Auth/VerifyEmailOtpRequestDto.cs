using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexHire.Application.DTOs.Auth;

public sealed class VerifyEmailOtpRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Otp { get; set; } = string.Empty;
}