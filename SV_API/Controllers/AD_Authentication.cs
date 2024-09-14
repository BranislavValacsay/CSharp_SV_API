﻿using LARS.Data;
using LARS.DTO;
using LARS.Interface;
using LARS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;

namespace LARS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AD_Authentication : ControllerBase
    {
        private readonly ITokenService _tokenservice;
        private readonly LarsContext _context;
        private readonly ILogJournal _journal;

        public AD_Authentication(ITokenService tokenservice, LarsContext context, ILogJournal journal)
        {
            _tokenservice = tokenservice;
            _context = context;
            _journal = journal;
        }

        [Authorize(AuthenticationSchemes = IISDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<AdUserDto>> Get()
        {            
            var userAccount = GetUserName();
            string GivenName = "";
            string Surname = "";

            var windowsIdentity = User.Identity as WindowsIdentity;
            var fullName = windowsIdentity.Name;

            using (var context = new PrincipalContext(ContextType.Domain))
            {
                var userPrincipal = UserPrincipal.FindByIdentity(context, fullName);

                if (userPrincipal != null)
                {
                    GivenName = userPrincipal.GivenName;
                    Surname = userPrincipal.Surname;
                }
            }

            AdminList adminRole = await _context.AdminList.FirstOrDefaultAsync();
            string role = "user";
            if (adminRole.UserName == userAccount)
            {
                role = "admin";
            }

            AdUserDto user = new AdUserDto();

            user.UserName = userAccount.Split('\\')[1];
            user.Domain = userAccount.Split('\\')[0];
            user.Surname = Surname;
            user.GivenName = GivenName;
            user.Role = role;
            var Token = _tokenservice.CreateToken(user);
            user.Token = await Token;

            Log log = new Log();
            log.MessageType = MessageType.Message;
            log.MessageBody = "Login detected : " + userAccount;
            if(user == null)
            {
                log.MessageType = MessageType.Error;
                log.MessageBody = "Login error. Logon not recognized " + User.Identity?.Name;
                await _journal.SendLog(log);
                return BadRequest("Error during logon process");
            }

            return new AdUserDto
            {
                UserName = user.UserName,
                Domain = user.Domain,
                Token = user.Token,
                Role = role,
                GivenName = GivenName,
                Surname = Surname
            };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("verifyToken")]
        public async Task<ActionResult<string>> VerifyToken()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"];
            var token = "";
            if (authorizationHeader == StringValues.Empty)
            {
                return Unauthorized();
            }
            token = authorizationHeader.ToString().Split(' ').Last();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokens = handler.ReadToken(token) as JwtSecurityToken;

            var header = tokens.Header;
            var payload = tokens.Payload;
            
            return Ok(payload);
        }

        private string GetUserName()
        {
            var userAccount = User.Identity?.Name;
            return userAccount;
        }
    }
}
