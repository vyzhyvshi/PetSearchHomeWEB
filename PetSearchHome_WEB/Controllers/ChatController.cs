using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly DeleteChatMessageUseCase _deleteChatMessageUseCase;
        private readonly BlockChatUserUseCase _blockChatUserUseCase;
        private readonly UnblockChatUserUseCase _unblockChatUserUseCase;
        private readonly IUserRepository _users;
        private readonly IStorageGateway _storageGateway;

        public ChatController(
            ILogger<ChatController> logger,
            StartChatUseCase startChatUseCase,
            ListChatConversationsUseCase listChatConversationsUseCase,
            GetChatThreadUseCase getChatThreadUseCase,
            SendChatMessageUseCase sendChatMessageUseCase,
            DeleteChatMessageUseCase deleteChatMessageUseCase,
            BlockChatUserUseCase blockChatUserUseCase,
            UnblockChatUserUseCase unblockChatUserUseCase,
            IUserRepository users,
            IStorageGateway storageGateway)
        {
            _logger = logger;
            _startChatUseCase = startChatUseCase;
            _listChatConversationsUseCase = listChatConversationsUseCase;
            _getChatThreadUseCase = getChatThreadUseCase;
            _sendChatMessageUseCase = sendChatMessageUseCase;
            _deleteChatMessageUseCase = deleteChatMessageUseCase;
            _blockChatUserUseCase = blockChatUserUseCase;
            _unblockChatUserUseCase = unblockChatUserUseCase;
            _users = users;
            _storageGateway = storageGateway;
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
                var lastMessageText = string.IsNullOrWhiteSpace(summary.LastMessage?.Content)
                    ? (summary.LastMessage?.ImageUrl is null ? null : "Фото")
                    : summary.LastMessage?.Content;
                items.Add(new ChatConversationItemViewModel
                {
                    ConversationId = summary.Conversation.Id,
                    OtherUserId = otherUserId,
                    OtherDisplayName = otherUser?.DisplayName ?? "Користувач",
                    LastMessage = lastMessageText,
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
                IsBlockedByMe = result.Value.IsBlockedByMe,
                IsBlockedByOther = result.Value.IsBlockedByOther,
                Messages = result.Value.Messages.Select(message => new ChatMessageViewModel
                {
                    MessageId = message.Id,
                    SenderId = message.SenderId,
                    Content = message.Content,
                    ImageUrl = message.ImageUrl,
                    SentAt = message.SentAt,
                    IsMine = message.SenderId == authContext.UserId,
                    CanDelete = message.SenderId == authContext.UserId
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Guid id, string message, IFormFile? photo, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            string? imageUrl = null;
            if (photo is not null && photo.Length > 0)
            {
                await using var stream = photo.OpenReadStream();
                imageUrl = await _storageGateway.UploadAsync(photo.FileName, stream, cancellationToken);
            }

            var result = await _sendChatMessageUseCase.ExecuteAsync(new SendChatMessageRequest(id, message, imageUrl), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося надіслати повідомлення.");
            }

            return RedirectToAction(nameof(Thread), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(Guid id, Guid messageId, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _deleteChatMessageUseCase.ExecuteAsync(new DeleteChatMessageRequest(messageId), authContext, cancellationToken);

            if (!result.IsSuccess || result.Value == null)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося видалити повідомлення.");
                return RedirectToAction(nameof(Thread), new { id });
            }

            if (!string.IsNullOrWhiteSpace(result.Value.ImageUrl))
            {
                await _storageGateway.DeleteAsync(result.Value.ImageUrl, cancellationToken);
            }

            return RedirectToAction(nameof(Thread), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(Guid id, Guid otherUserId, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _blockChatUserUseCase.ExecuteAsync(new BlockChatUserRequest(otherUserId), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося заблокувати користувача.");
            }

            return RedirectToAction(nameof(Thread), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(Guid id, Guid otherUserId, CancellationToken cancellationToken)
        {
            var authContext = await GetAuthContextAsync(cancellationToken);
            var result = await _unblockChatUserUseCase.ExecuteAsync(new UnblockChatUserRequest(otherUserId), authContext, cancellationToken);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage ?? "Не вдалося розблокувати користувача.");
            }

            return RedirectToAction(nameof(Thread), new { id });
        }
    }
}
