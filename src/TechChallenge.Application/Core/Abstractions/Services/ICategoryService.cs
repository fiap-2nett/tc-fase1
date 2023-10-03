using System.Collections.Generic;
using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Category;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ICategoryService
    {
        Task<DetailedCategoryResponse> GetByIdAsync(int idCategory);
        Task<IEnumerable<DetailedCategoryResponse>> GetAsync();
    }
}
