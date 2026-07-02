using DanceSchool.Business.Interfaces.Chat;
using DanceSchool.Business.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DanceSchool.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost("conversations/direct")]
        public async Task<IActionResult> CreateDirect(CreateDirectConversationRequest request)
        {
            var result = await _chatService.CreateDirectConversation(CurrentUserId, request);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpPost("conversations/group")]
        public async Task<IActionResult> CreateGroup(CreateGroupConversationRequest request)
        {
            var result = await _chatService.CreateGroupConversation(CurrentUserId, request);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }

        [HttpPost("conversations/{conversationId}/members")]
        public async Task<IActionResult> AddMember(Guid conversationId, [FromBody] AddMemberRequest request)
        {
            var result = await _chatService.AddMember(CurrentUserId, conversationId, request.UserId);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok();
        }



        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var result = await _chatService.GetMyConversations(CurrentUserId);
            return Ok(result.Value);
        }

        [HttpGet("conversations/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(Guid conversationId)
        {
            var result = await _chatService.GetMessages(CurrentUserId, conversationId);
            return result.IsFailed
                ? BadRequest(new { errors = result.Errors.Select(e => e.Message) })
                : Ok(result.Value);
        }
    }
}