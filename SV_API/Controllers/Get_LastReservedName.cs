using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sp_api.Data;
using sp_api.Models;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Get_LastReservedName : ControllerBase
    {
        private readonly API_DbContext _context;

        public Get_LastReservedName(API_DbContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("{guid}")]
        public async Task<ActionResult<ServerName>> GetReserved(string guid)
        {
            RequestServer? request = await _context.RequestServers
                .Where(x => x.Guid == guid)
                .FirstOrDefaultAsync();

            if (request == null)
            {
                return NotFound();
            }

            if (!request.ServerName.IsNullOrEmpty())
            {
                ServerName returnName = new ServerName();
                returnName.Id = 0;

                string temp = Regex.Replace(request.ServerName, @"\D", "");

                returnName.Name = Convert.ToInt16(temp);
                return returnName;
            }
            
            else
            {
                string highestServerName = _context.RequestServers.Where(a => (a.ServerName != "")).Max(x => (string)x.ServerName) ?? "";
                int highestServerValue = Convert.ToInt16(Regex.Replace(highestServerName, @"\D", ""));

                int highestServerNameReservation = _context.ServerNameTable.Max(x => (int?)x.Name) ?? 0;


                ServerName returnName = new ServerName();
                returnName.Id = 0;
                returnName.Name = CompareNumbers(highestServerValue, highestServerNameReservation);

                returnName.Name++;
                return Ok(returnName);
            }
           
        }

        private int CompareNumbers(int a, int b)
        {
            return Math.Max(a, b);
        }
    }
}
