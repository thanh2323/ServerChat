using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Auth;

namespace DocuMind.Application.Interface.IAuth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto dto);

    }
}
