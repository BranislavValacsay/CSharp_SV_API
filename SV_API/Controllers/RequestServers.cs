using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sp_api.Data;
using sp_api.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestServers : ControllerBase
    {
        private readonly API_DbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogJournal _journal;
        private readonly Log _log;

        public RequestServers(API_DbContext context, IMapper mapper, ILogJournal journal, Log log)
        {
            _context = context;
            _mapper = mapper;
            _journal = journal;
            _log = log;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestServerDTO>>> GetRequestServers([FromQuery] FilterParams filterParams)
        {
          if (_context.RequestServers == null)
          {
              return NotFound();
          }
            var result = _context.RequestServers
                .Include(x => x.VMMNetwork)
                .Include(vmm => vmm.VMMServer.Location)
                .Include(wv => wv.WindowsVersion)
                .OrderBy(x => x.CreationTime)
                .Reverse();
            if (filterParams.Key != null) {
                result = result.Where(
                    x => x.ServerName.Contains(filterParams.Key) ||
                    x.Domain.Contains(filterParams.Key) ||
                    x.Requester.Contains(filterParams.Key) || 
                    x.VMMNetwork.Name.Contains(filterParams.Key) ||
                    x.VMMServer.Location.Name.Contains(filterParams.Key) ||
                    x.WindowsVersion.Name.Contains(filterParams.Key) ||
                    x.VMMNetwork.VlanID.ToString().Contains(filterParams.Key) ||
                    x.BlimpName.Contains(filterParams.Key) ||
                    x.BlimpId.ToString().Contains(filterParams.Key)
                    );
            }
             
            var convert = await result.ToListAsync();
            List<RequestServerDTO> convert2Dto = _mapper.Map<List<RequestServerDTO>>(convert);
            return Ok(convert2Dto);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{guid}")]
        public async Task<ActionResult<RequestServerDTO>> GetRequestServer(string guid)
        {
          if (_context.RequestServers == null)
          {
              return NotFound();
          }
            var requestServer = await _context.RequestServers
                .Where(x => x.Guid == guid)
                .Include(x => x.VMMNetwork)
                .Include(vmm => vmm.VMMServer.Location)
                .Include(wv => wv.WindowsVersion)
                .FirstOrDefaultAsync();
            if (requestServer == null)
            {
                return NotFound();
            }
            RequestServerDTO requsetServerDTO = _mapper.Map<RequestServerDTO>(requestServer);
            return requsetServerDTO;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpPut("{Guid}")]
        public async Task<IActionResult> PutRequestServer(string guid, PUT_RequestServer_DTO Put_RequestServerDto)
        {
            if (guid != Put_RequestServerDto.Guid)
            {
                _log.MessageType = MessageType.Error;
                _log.MessageBody = "Request edit: There is no server found in database to process.";
                _log.Guid = guid;
                await _journal.SendLog(_log);
                return BadRequest();
            }
            var requestServerToModify = await _context.RequestServers.FirstOrDefaultAsync(x => x.Guid == guid);
            _mapper.Map(Put_RequestServerDto, requestServerToModify);
            _context.Entry(requestServerToModify).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request edit: saving to database";
            _log.Result = JsonSerializer.Serialize(Put_RequestServerDto);
            _log.Guid = guid;
            await _journal.SendLog(_log);
            return NoContent();
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult<PUT_RequestServer_DTO>> PostRequestServer(PUT_RequestServer_DTO requestServer)
        {
            RequestServer newRequest = _mapper.Map<RequestServer>(requestServer);
            if(requestServer.Guid.IsNullOrEmpty()) { newRequest.Guid = Guid.NewGuid().ToString(); }
            if(!requestServer.LeonRequestId.IsNullOrEmpty()) { newRequest.Guid = requestServer.LeonRequestId; }
            newRequest.CreationTime = DateTime.Now;
            _context.RequestServers.Add(newRequest);
            await _context.SaveChangesAsync();
            Log log = new()
            {
                MessageBody = "Request: new server",
                Guid = newRequest.Guid,
                MessageType = MessageType.Message,
                Result = JsonSerializer.Serialize(newRequest)
            };
            await _journal.SendLog(log);
            return CreatedAtAction("GetRequestServer", new { Guid = newRequest.Guid, CreationTime = newRequest.CreationTime }, newRequest);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpDelete("{guid}")]
        public async Task<IActionResult> DeleteRequestServer(string guid)
        {
            if (_context.RequestServers == null)
            {
                return NotFound();
            }
            var requestServer = await _context.RequestServers.Where(s => s.Guid == guid).FirstOrDefaultAsync();
            if (requestServer == null)
            {
                return NotFound();
            }
            _context.RequestServers.Remove(requestServer);
            await _context.SaveChangesAsync();
            Log log = new Log
            {
                MessageBody = "Request: request for server deleted",
                Guid = guid,
                MessageType = MessageType.Message
            };
            await _journal.SendLog(log);
            return NoContent();
        }
        private bool RequestServerExists(string guid)
        {
            return (_context.RequestServers?.Any(e => e .Guid == guid)).GetValueOrDefault();
        }
    }
}
