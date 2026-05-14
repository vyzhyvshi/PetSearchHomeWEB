using Moq;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Chat;
using PetSearchHome_WEB.Application.Notifications;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetSearchHome_WEB.Tests.Application
{
    public class ApplicationUseCasesTests
    {
        // 1. ТЕСТИ: ClearChatHistoryUseCase
        [Fact]
        public async Task ClearChatHistory_WhenAuthorizedAndParticipant_ReturnsSuccess()
        {
            var chatRepoMock = new Mock<IChatRepository>();
            var userId = new int();
            var conversationId = new int();
            var authContext = new AuthContext { UserId = userId };

            var conversation = new ChatConversation { Id = conversationId, UserAId = userId, UserBId = new int() };
            chatRepoMock.Setup(r => r.GetConversationByIdAsync(conversationId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(conversation);

            var useCase = new ClearChatHistoryUseCase(chatRepoMock.Object);
            var result = await useCase.ExecuteAsync(new ClearChatHistoryRequest(conversationId), authContext);

            Assert.True(result.IsSuccess);
            chatRepoMock.Verify(r => r.ClearHistoryAsync(conversationId, userId, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        }

		[Fact]
		public async Task ClearChatHistory_WhenNotParticipant_ReturnsFailure()
		{
			var chatRepoMock = new Mock<IChatRepository>();
			var userId = 99; 
			var conversationId = 1;
			var authContext = new AuthContext { UserId = userId, Role = Role.Person };

			var conversation = new ChatConversation
			{
				Id = conversationId,
				UserAId = 1, 
				UserBId = 2 
			};

			chatRepoMock.Setup(r => r.GetConversationByIdAsync(conversationId, It.IsAny<CancellationToken>()))
				.ReturnsAsync(conversation);

			var useCase = new ClearChatHistoryUseCase(chatRepoMock.Object);
			var result = await useCase.ExecuteAsync(new ClearChatHistoryRequest(conversationId), authContext);

			Assert.False(result.IsSuccess);
			Assert.Equal("Немає доступу до чату.", result.ErrorMessage);
		}

		// 2. ТЕСТИ: DeleteAccountUseCase
		[Fact]
        public async Task DeleteAccount_WithCorrectPassword_ReturnsSuccess()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var userId = new int();
            var authContext = new AuthContext { UserId = userId };
            var currentPassword = "myPassword123";

            var user = new User { Id = userId, PasswordHash = "hashedPassword" };
            userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            hasherMock.Setup(h => h.Verify(currentPassword, user.PasswordHash)).Returns(true);

            var useCase = new DeleteAccountUseCase(userRepoMock.Object, hasherMock.Object);
            var result = await useCase.ExecuteAsync(new DeleteAccountRequest(currentPassword), authContext);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
            userRepoMock.Verify(r => r.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAccount_WithWrongPassword_ReturnsFailure()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var userId = new int();
            var authContext = new AuthContext { UserId = userId };

            var user = new User { Id = userId, PasswordHash = "hashedPassword" };
            userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            hasherMock.Setup(h => h.Verify(It.IsAny<string>(), user.PasswordHash)).Returns(false);

            var useCase = new DeleteAccountUseCase(userRepoMock.Object, hasherMock.Object);
            var result = await useCase.ExecuteAsync(new DeleteAccountRequest("wrongPass"), authContext);

            Assert.False(result.IsSuccess);
            Assert.Equal("Поточний пароль неправильний.", result.ErrorMessage);
        }

        // 3. ТЕСТИ: ChangePasswordUseCase
        [Fact]
        public async Task ChangePassword_WithValidData_ReturnsSuccess()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var userId = new int();
            var authContext = new AuthContext { UserId = userId };

            var user = new User { Id = userId, PasswordHash = "oldHash" };
            userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            hasherMock.Setup(h => h.Verify("oldPass", user.PasswordHash)).Returns(true);
            hasherMock.Setup(h => h.Hash("newPass")).Returns("newHash");

            var useCase = new ChangePasswordUseCase(userRepoMock.Object, hasherMock.Object);
            var result = await useCase.ExecuteAsync(new ChangePasswordRequest("oldPass", "newPass"), authContext);

            Assert.True(result.IsSuccess);
            userRepoMock.Verify(r => r.UpdatePasswordAsync(userId, "newHash", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WhenNotAuthorized_ReturnsFailure()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var authContext = new AuthContext { UserId = null };

            var useCase = new ChangePasswordUseCase(userRepoMock.Object, hasherMock.Object);
            var result = await useCase.ExecuteAsync(new ChangePasswordRequest("old", "new"), authContext);

            Assert.False(result.IsSuccess);
            Assert.Equal("Потрібна авторизація.", result.ErrorMessage);
        }

        
        // 4. ТЕСТИ: ResetPasswordUseCase
        [Fact]
        public async Task ResetPassword_WithValidToken_ReturnsSuccess()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var tokenRepoMock = new Mock<IPasswordResetTokenRepository>();
            var email = "test@domain.com";
            var userId = new int();

            var user = new User { Id = userId, Email = email };
            var resetToken = new PasswordResetToken { Id = new int(), UserId = userId };

            userRepoMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            tokenRepoMock.Setup(r => r.GetUsableAsync(userId, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(resetToken);
            hasherMock.Setup(h => h.Hash("newPass")).Returns("newHashedPass");

            var useCase = new ResetPasswordUseCase(userRepoMock.Object, hasherMock.Object, tokenRepoMock.Object);
            var result = await useCase.ExecuteAsync(new ResetPasswordRequest(email, "rawToken", "newPass"), new AuthContext());

            Assert.True(result.IsSuccess);
            userRepoMock.Verify(r => r.UpdatePasswordAsync(userId, "newHashedPass", It.IsAny<CancellationToken>()), Times.Once);
            tokenRepoMock.Verify(r => r.MarkUsedAsync(resetToken.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_WhenUserNotFound_ReturnsFailure()
        {
            var userRepoMock = new Mock<IUserRepository>();
            var hasherMock = new Mock<IPasswordHasher>();
            var tokenRepoMock = new Mock<IPasswordResetTokenRepository>();

            userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);

            var useCase = new ResetPasswordUseCase(userRepoMock.Object, hasherMock.Object, tokenRepoMock.Object);
            var result = await useCase.ExecuteAsync(new ResetPasswordRequest("notfound@domain.com", "token", "newPass"), new AuthContext());

            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.ErrorMessage);
        }

        // 5. ТЕСТИ: GetUserNotificationsUseCase
        [Fact]
        public async Task GetUserNotifications_WhenAuthorized_ReturnsNotificationList()
        {
            var notifRepoMock = new Mock<INotificationRepository>();
            var userId = new int();
            var authContext = new AuthContext { UserId = userId };

            var notifications = new List<Notification>
            {
                new Notification { Id = 1, RecipientId = userId, Message = "Test 1", IsRead = false, CreatedAt = DateTimeOffset.UtcNow }
            };

            notifRepoMock.Setup(r => r.GetByRecipientIdAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(notifications);

            var useCase = new GetUserNotificationsUseCase(notifRepoMock.Object);
            var result = await useCase.ExecuteAsync(new GetUserNotificationsRequest(), authContext);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value!);
            Assert.Equal("Test 1", result.Value!.First().Message);
        }

        [Fact]
        public async Task GetUserNotifications_WhenNotAuthorized_ReturnsFailure()
        {
            var notifRepoMock = new Mock<INotificationRepository>();
            var authContext = new AuthContext { UserId = null };

            var useCase = new GetUserNotificationsUseCase(notifRepoMock.Object);
            var result = await useCase.ExecuteAsync(new GetUserNotificationsRequest(), authContext);

            Assert.False(result.IsSuccess);
            Assert.Equal("Потрібна авторизація для перегляду сповіщень.", result.ErrorMessage);
        }
    }
}