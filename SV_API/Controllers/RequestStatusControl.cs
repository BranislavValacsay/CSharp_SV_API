using AutoMapper;
using sp_api.Data;
using sp_api.Interface;
using sp_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace sp_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestStatusControl : ControllerBase
    {
        private readonly API_DbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogJournal _journal;
        private readonly Log _log;
        private readonly IOrchestrator _orchestrator;

        public RequestStatusControl(API_DbContext context, IMapper mapper,ILogJournal journal, Log log,IOrchestrator orchestrator )
        {
            _context = context;
            _mapper = mapper;
            _journal = journal;
            _log = log;
            _orchestrator = orchestrator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("approve_request/{guid}")]
        public async Task<ActionResult> ApproveRequest(string guid)
        {
            await _orchestrator.RequestStatusChange(guid, 10);

            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: Approving request";
            _log.Guid = guid;
            await _journal.SendLog(_log);
            return Ok();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("mark_complete_request/{guid}")]
        public async Task<ActionResult> MarkCompleteRequest(string guid)
        {
            await _orchestrator.RequestStatusChange(guid, 100);

            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: request marked as complete";
            _log.Guid = guid;
            await _journal.SendLog(_log);
            return Ok(new { message = "Request marked as complete" + guid, type = "ok" });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        [HttpGet("manual_override_request/{guid}")]
        public async Task<ActionResult> ManualOverrideRequest(string guid)
        {

            RequestServer request = await _context.RequestServers.Where(g => g.Guid == guid).FirstOrDefaultAsync();

            request.ManualOverride = true;
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            _log.MessageType = MessageType.Message;
            _log.MessageBody = "Request: Manual override. Orchestrator will NOT continue automatically from now on.";
            _log.Guid = guid;
            await _journal.SendLog(_log);
            return Ok(new { message = "Manual override for: " + guid, type = "ok" });
        }

    }
}
