using sp_api.Models;

namespace sp_api.DTO
{
    public class AdUserDto : AdUser
    {
        public string Role { get; set; }
        public string Token { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
    }
}
