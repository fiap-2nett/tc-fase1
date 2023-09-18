using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Authentication;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Users;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface IUserService
    {
        #region IUserService Members

        Task<DetailedUserResponse> GetUserByIdAsync(int idUser);
        Task<PagedList<UserResponse>> GetUsersAsync(GetUsersRequest request);
        Task<TokenResponse> CreateAsync(string name, string surname, string email, string password);
        Task ChangePasswordAsync(int idUser, string password);
        Task UpdateUserAsync(int idUser, string name, string surname);

        #endregion
    }
}
