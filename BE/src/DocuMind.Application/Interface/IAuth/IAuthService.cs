using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Auth;
using DocuMind.Application.DTOs.Common;

namespace DocuMind.Application.Interface.IAuth
{
    public interface IAuthService 
    {
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<AuthResponseDto>> ChangePasswordAsync(ChangePasswordDto dto);


    }
}
