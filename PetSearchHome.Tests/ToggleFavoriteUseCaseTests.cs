using Moq;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class ToggleFavoriteUseCaseTests
    {
        private readonly Mock<IFavoriteRepository> _favoriteRepoMock;
        private readonly ToggleFavoriteUseCase _useCase;

        public ToggleFavoriteUseCaseTests()
        {
            _favoriteRepoMock = new Mock<IFavoriteRepository>();
            _useCase = new ToggleFavoriteUseCase(_favoriteRepoMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenNotAuthorized_ReturnsFailure()
        {
            var request = new ToggleFavoriteRequest(Guid.NewGuid());
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("авторизація", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenAlreadyFavorited_RemovesAndReturnsFalse()
        {
            var userId = Guid.NewGuid();
            var listingId = Guid.NewGuid();
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };
            var request = new ToggleFavoriteRequest(listingId);

            _favoriteRepoMock
                .Setup(repo => repo.GetAsync(userId, listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Favorite { UserId = userId, ListingId = listingId });

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
            _favoriteRepoMock.Verify(repo => repo.RemoveAsync(userId, listingId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}