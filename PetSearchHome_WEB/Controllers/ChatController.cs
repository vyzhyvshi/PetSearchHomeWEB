using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetSearchHome_WEB.Application.Chat;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Models.Chat;
using PetSearchHome_WEB.Security;

namespace PetSearchHome_WEB.Controllers
{
    [Authorize(Roles = RoleNames.AuthenticatedUser)]
    public class ChatController : AppController
    {
        private readonly ILogger<ChatController> _logger;
        private readonly StartChatUseCase _startChatUseCase;
        private readonly ListChatConversationsUseCase _listChatConversationsUseCase;
        private readonly GetChatThreadUseCase _getChatThreadUseCase;
        private readonly SendChatMessageUseCase _sendChatMessageUseCase;
        private readonly IUserRepository _users;

        public ChatController(
            ILogger<ChatController> logger,
            StartChatUseCase startChatUseCase,
            ListChatConversationsUseCase listChatConversationsUseCase,
            GetChatThreadUseCase getChatThreadUseCase,
            SendChatMessageUseCase sendChatMessageUseCase,
            IUserRepository users)
        {
            _logger = logger;
            _startChatUseCase = startChatUseCase;
            _listChatConversationsUseCase = listChatConversationsUseCase;
            _getChatThreadUseCase = getChatThreadUseCase;
            _sendChatMessageUseCase = sendChatMessageUseCase;
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _listChatConversationsUseCase.ExecuteAsync(new ListChatConversationsRequest(), authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                return Forbid();
            }

            var items = new List<ChatConversationItemViewModel>();
            foreach (var summary in result.Value)
            {
                var otherUserId = summary.Conversation.GetOtherParticipant(authContext.UserId!.Value);
                var otherUser = await _users.GetByIdAsync(otherUserId, cancellationToken);
                items.Add(new ChatConversationItemViewModel
                {
                    ConversationId = summary.Conversation.Id,
                    OtherUserId = otherUserId,
                    OtherDisplayName = otherUser?.DisplayName ?? "Користувач",
                    LastMessage = summary.LastMessage?.Content,
                    LastMessageAt = summary.LastMessage?.SentAt
                });
            }

            var model = new ChatOverviewViewModel
            {
                Conversations = items
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _startChatUseCase.ExecuteAsync(new StartChatRequest(id), authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося відкрити чат.");
                return RedirectToAction("Details", "Profile", new { id });
            }

            return RedirectToAction(nameof(Thread), new { id = result.Value.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Thread(Guid id, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _getChatThreadUseCase.ExecuteAsync(new GetChatThreadRequest(id), authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                SetErrorMessage(result.ErrorMessage ?? "Чат не знайдено.");
                return RedirectToAction(nameof(Index));
            }

            var otherUserId = result.Value.Conversation.GetOtherParticipant(authContext.UserId!.Value);
            var otherUser = await _users.GetByIdAsync(otherUserId, cancellationToken);

            var model = new ChatThreadViewModel
            {
                ConversationId = result.Value.Conversation.Id,
                OtherUserId = otherUserId,
                OtherDisplayName = otherUser?.DisplayName ?? "Користувач",
                Messages = result.Value.Messages.Select(message => new ChatMessageViewModel
                {
                    SenderId = message.SenderId,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsMine = message.SenderId == authContext.UserId
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Guid id, string message, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _sendChatMessageUseCase.ExecuteAsync(new SendChatMessageRequest(id, message), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося надіслати повідомлення.");
            }

            return RedirectToAction(nameof(Thread), new { id });
        }
    }
}
