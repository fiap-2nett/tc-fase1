using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Authentication;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface IAuthenticationService
    {
        #region IAuthenticationService Members

        Task<TokenResponse> Login(string email, string password);

        #endregion
    }
}
