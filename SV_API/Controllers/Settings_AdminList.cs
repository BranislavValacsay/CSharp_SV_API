using sp_api.Data;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Settings_AdminList : ControllerBase
    {
        private readonly API_DbContext _context;

        public Settings_AdminList(API_DbContext context)
        {
            _context = context;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet]
        public async Task<IEnumerable<AdminList>> getAdminList()
        {
            return await _context.AdminList.ToListAsync();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPost]
        public async Task<AdminList> setAdmin(string user)
        {
            AdminList dummy = new AdminList();
            return dummy;
        }
    }
}
