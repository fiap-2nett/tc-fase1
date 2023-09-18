using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Core.Abstractions.Authentication
{
    public interface IJwtProvider
    {
        #region IJwtProvider Members

        string Create(User user);

        #endregion
    }
}
