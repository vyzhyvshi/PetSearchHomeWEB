using Moq;
using PetSearchHome_WEB.Application.Chat;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using Xunit;

namespace PetSearchHome.Tests
{
    public class ChatUseCaseTests
    {
        private readonly AuthContext _validAuth = new AuthContext { UserId = Guid.NewGuid(), Role = PetSearchHome_WEB.Domain.ValueObjects.Role.Person };
        private readonly AuthContext _guestAuth = new AuthContext { UserId = null, Role = PetSearchHome_WEB.Domain.ValueObjects.Role.Guest };

        // 1. SendChatMessageUseCase

        [Fact]
        public async Task SendMessage_WhenValid_ReturnsSuccess()
        {
            var chatsMock = new Mock<IChatRepository>();
            var otherUserId = Guid.NewGuid();

            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                UserAId = _validAuth.UserId!.Value,
                UserBId = otherUserId
            };

            chatsMock.Setup(r => r.GetConversationByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            chatsMock.Setup(r => r.IsBlockedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var useCase = new SendChatMessageUseCase(chatsMock.Object);
            var result = await useCase.ExecuteAsync(new SendChatMessageRequest(Guid.NewGuid(), "Привіт!", "image_url.jpg"), _validAuth);

            Assert.True(result.IsSuccess);
            chatsMock.Verify(r => r.AddMessageAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMessage_WhenBlocked_ReturnsFailure()
        {
            var chatsMock = new Mock<IChatRepository>();
            var otherUserId = Guid.NewGuid();

            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                UserAId = _validAuth.UserId!.Value,
                UserBId = otherUserId
            };

            chatsMock.Setup(r => r.GetConversationByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            chatsMock.Setup(r => r.IsBlockedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var useCase = new SendChatMessageUseCase(chatsMock.Object);
            var result = await useCase.ExecuteAsync(new SendChatMessageRequest(Guid.NewGuid(), "Привіт!", null), _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("заблокований", result.ErrorMessage);
        }

        // 2. DeleteChatMessageUseCase

        [Fact]
        public async Task DeleteMessage_WhenSender_ReturnsSuccess()
        {
            var chatsMock = new Mock<IChatRepository>();
            var conversationId = Guid.NewGuid();

            var message = new ChatMessage { Id = Guid.NewGuid(), SenderId = _validAuth.UserId!.Value, ConversationId = conversationId };

            var conversation = new ChatConversation
            {
                Id = conversationId,
                UserAId = _validAuth.UserId!.Value,
                UserBId = Guid.NewGuid()
            };

            chatsMock.Setup(r => r.GetMessageByIdAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(message);
            chatsMock.Setup(r => r.GetConversationByIdAsync(message.ConversationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            var useCase = new DeleteChatMessageUseCase(chatsMock.Object);
            var result = await useCase.ExecuteAsync(new DeleteChatMessageRequest(message.Id), _validAuth);

            Assert.True(result.IsSuccess);
            chatsMock.Verify(r => r.DeleteMessageAsync(message.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WhenNotSender_ReturnsFailure()
        {
            var chatsMock = new Mock<IChatRepository>();
            var conversationId = Guid.NewGuid();

            var message = new ChatMessage { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ConversationId = conversationId };

            var conversation = new ChatConversation
            {
                Id = conversationId,
                UserAId = _validAuth.UserId!.Value,
                UserBId = Guid.NewGuid()
            };

            chatsMock.Setup(r => r.GetMessageByIdAsync(message.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(message);
            chatsMock.Setup(r => r.GetConversationByIdAsync(message.ConversationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            var useCase = new DeleteChatMessageUseCase(chatsMock.Object);
            var result = await useCase.ExecuteAsync(new DeleteChatMessageRequest(message.Id), _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("лише свої", result.ErrorMessage);
        }

        // 3. ListChatConversationsUseCase

        [Fact]
        public async Task ListConversations_WhenAuthorized_ReturnsList()
        {
            var chatsMock = new Mock<IChatRepository>();
            chatsMock.Setup(r => r.ListConversationsAsync(_validAuth.UserId!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ChatConversation> { new ChatConversation() });

            var useCase = new ListChatConversationsUseCase(chatsMock.Object);
            var result = await useCase.ExecuteAsync(new ListChatConversationsRequest(), _validAuth);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!);
        }

        [Fact]
        public async Task ListConversations_WhenGuest_ReturnsFailure()
        {
            var useCase = new ListChatConversationsUseCase(new Mock<IChatRepository>().Object);
            var result = await useCase.ExecuteAsync(new ListChatConversationsRequest(), _guestAuth);

            Assert.False(result.IsSuccess);
        }

        // 4. SendChatMessageUseCase 

        // Перевіряє, що повідомлення успішно відправляється, якщо користувач прикріпив лише фото (без тексту).
        [Fact]
        public async Task SendMessage_WithPhotoOnly_ReturnsSuccess()
        {
            var chatsMock = new Mock<IChatRepository>();
            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                UserAId = _validAuth.UserId!.Value,
                UserBId = Guid.NewGuid()
            };

            chatsMock.Setup(r => r.GetConversationByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            chatsMock.Setup(r => r.IsBlockedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var useCase = new SendChatMessageUseCase(chatsMock.Object);

            var result = await useCase.ExecuteAsync(new SendChatMessageRequest(Guid.NewGuid(), "", "photo.png"), _validAuth);

            Assert.True(result.IsSuccess);
            chatsMock.Verify(r => r.AddMessageAsync(It.Is<ChatMessage>(m => m.ImageUrl == "photo.png" && m.Content == string.Empty), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendMessage_WithEmptyTextAndNoPhoto_ReturnsFailure()
        {
            var chatsMock = new Mock<IChatRepository>();
            var conversation = new ChatConversation
            {
                Id = Guid.NewGuid(),
                UserAId = _validAuth.UserId!.Value,
                UserBId = Guid.NewGuid()
            };

            chatsMock.Setup(r => r.GetConversationByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(conversation);

            var useCase = new SendChatMessageUseCase(chatsMock.Object);

            var result = await useCase.ExecuteAsync(new SendChatMessageRequest(Guid.NewGuid(), "   ", null), _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("порожнім", result.ErrorMessage);
        }


        // 5. BlockChatUserUseCase

        [Fact]
        public async Task BlockUser_WhenValid_ReturnsSuccess()
        {
            var chatsMock = new Mock<IChatRepository>();
            var useCase = new BlockChatUserUseCase(chatsMock.Object);

            var targetUserId = Guid.NewGuid();
            var result = await useCase.ExecuteAsync(new BlockChatUserRequest(targetUserId), _validAuth);

            Assert.True(result.IsSuccess);
            chatsMock.Verify(r => r.SetBlockedAsync(_validAuth.UserId!.Value, targetUserId, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task BlockUser_WhenTargetIsSelf_ReturnsFailure()
        {
            var useCase = new BlockChatUserUseCase(new Mock<IChatRepository>().Object);

            var result = await useCase.ExecuteAsync(new BlockChatUserRequest(_validAuth.UserId!.Value), _validAuth);

            Assert.False(result.IsSuccess);
            Assert.Contains("самого себе", result.ErrorMessage);
        }
    }
}