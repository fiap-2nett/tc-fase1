using System.Threading.Tasks;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.ValueObjects;

namespace TechChallenge.Domain.Repositories
{
    public interface IUserRepository
    {
        #region IUserRepository Members

        Task<User> GetByIdAsync(int idUser);
        Task<User> GetByEmailAsync(Email email);
        Task<bool> IsEmailUniqueAsync(Email email);
        void Insert(User user);

        #endregion
    }
}
