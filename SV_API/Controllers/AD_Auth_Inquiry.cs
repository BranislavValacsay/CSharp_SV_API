using sp_api.Data;
using sp_api.DTO;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.AccountManagement;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AD_Auth_Inquiry : ControllerBase
    {
        private readonly ITokenService _tokenservice;
        private readonly API_DbContext _context;

        public AD_Auth_Inquiry(ITokenService tokenservice, API_DbContext context)
        {
            _tokenservice = tokenservice;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Authenticate([FromBody] CredentialsDto credentials)
        {
            // Assuming credentials.Username and credentials.Password contain the input
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return BadRequest("Username and password are required.");
            }

            bool isValid = false;
            string message = "";
            var user = new AdUserDto();

            // Use PrincipalContext with ContextType.Domain for Active Directory authentication
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                isValid = context.ValidateCredentials(credentials.Username, credentials.Password);
            }

            if (!isValid)
            {
                message = "Error: bad username or password.";
                return BadRequest(message);
            }


            user.UserName = credentials.Username.Split('\\')[1]; 
            user.Domain = credentials.Username.Split('\\')[0];
            user.Role = "user";
            user.GivenName = "";
            user.Surname = "";

            using (var context = new PrincipalContext(ContextType.Domain))
            {
                var userPrincipal = UserPrincipal.FindByIdentity(context, credentials.Username);
                
                if (userPrincipal != null)
                {
                    user.GivenName = userPrincipal.GivenName;
                    user.Surname = userPrincipal.Surname;

                }
                
            }

            AdminList? adminRole = await _context.AdminList
                .Where(u => u.UserName == credentials.Username)
                .FirstOrDefaultAsync();

                if(adminRole != null)
                {
                    user.Role = "admin";
                }

            
            var Token = _tokenservice.CreateToken(user);
            user.Token = await Token;

            return Ok(user);
        }

    }
    public class CredentialsDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

}
