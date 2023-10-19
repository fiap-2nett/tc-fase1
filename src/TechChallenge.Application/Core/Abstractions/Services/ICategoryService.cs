using System.Threading.Tasks;
using System.Collections.Generic;
using TechChallenge.Application.Contracts.Category;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ICategoryService
    {
        #region ICategoryService Members

        Task<IEnumerable<CategoryResponse>> GetAsync();
        Task<DetailedCategoryResponse> GetByIdAsync(int idCategory);

        #endregion
    }
}
