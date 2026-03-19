using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IAuthTokenService
    {
        string IssueToken(User user);
    }
}
