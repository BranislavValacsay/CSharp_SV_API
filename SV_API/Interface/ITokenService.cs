using sp_api.DTO;
using sp_api.Models;

namespace sp_api.Interface
{
    public interface ITokenService
    {
        Task<string> CreateToken(AdUserDto user);
    }
}
