using Moq;
using Xunit;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome.Tests
{
    public class RegisterShelterUseCaseTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly RegisterShelterUseCase _useCase;

        public RegisterShelterUseCaseTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _useCase = new RegisterShelterUseCase(_userRepoMock.Object, _passwordHasherMock.Object);
        }


        [Fact]
        public async Task ExecuteAsync_NewShelter_CreatesSuccessfully()
        {
            var request = new RegisterShelterRequest("shelter@test.com", "Happy Paws", "password123");
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((User?)null);

            _passwordHasherMock.Setup(hasher => hasher.Hash(request.Password))
                               .Returns("hashed_pass");


            await _useCase.ExecuteAsync(request, authContext, CancellationToken.None);


            _userRepoMock.Verify(repo =>
                repo.AddAsync(It.Is<User>(u => u.Email == request.Email && u.Role == Role.Shelter), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_EmailAlreadyExists_ThrowsException()
        {
            var request = new RegisterShelterRequest("exist@test.com", "Happy Paws", "password123");
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = request.Email });


            await Assert.ThrowsAnyAsync<Exception>(() =>
                _useCase.ExecuteAsync(request, authContext, CancellationToken.None));
        }
    }
}