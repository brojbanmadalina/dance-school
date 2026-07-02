using System.Security.Claims;
using DanceSchool.Business.Interfaces.Chat;
using DanceSchool.Business.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DanceSchool.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid CurrentUserId =>
            Guid.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public override async Task OnConnectedAsync()
        {
            var conversationsResult = await _chatService.GetMyConversations(CurrentUserId);

            if (conversationsResult.IsSuccess)
            {
                foreach (var conv in conversationsResult.Value)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(Guid conversationId, string content)
        {
            var result = await _chatService.SaveMessage(CurrentUserId, new SendMessageRequest
            {
                ConversationId = conversationId,
                Content = content
            });

            if (result.IsFailed)
            {
                await Clients.Caller.SendAsync("Error", result.Errors.First().Message);
                return;
            }

            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", result.Value);
        }

        public async Task Typing(Guid conversationId)
        {
            await Clients.OthersInGroup(conversationId.ToString())
                .SendAsync("UserTyping", CurrentUserId);
        }


    }
}