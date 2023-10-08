using System.Threading.Tasks;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Repositories
{
    public interface ICategoryRepository
    {
        #region ICategoryRepository Members

        Task<Category> GetByIdAsync(int idCategory);

        #endregion
    }
}
