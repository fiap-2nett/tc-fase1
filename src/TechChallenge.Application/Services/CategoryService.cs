using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Application.Contracts.Category;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Application.Services
{
    internal sealed class CategoryService : ICategoryService
    {
        #region Read-Only Fields

        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public CategoryService(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #endregion

        #region ICategoryService Members

        public async Task<IEnumerable<CategoryResponse>> GetAsync()
        {
            IQueryable<CategoryResponse> categoriesQuery = (
                from category in _dbContext.Set<Category, int>().AsNoTracking()
                select new CategoryResponse
                {
                    IdCategory = category.Id,
                    Name = category.Name
                }
            );

            return await categoriesQuery.ToListAsync();
        }

        public async Task<DetailedCategoryResponse> GetByIdAsync(int idCategory)
        {
            IQueryable<DetailedCategoryResponse> categoriesQuery = (
                from category in _dbContext.Set<Category, int>().AsNoTracking()
                join priority in _dbContext.Set<Priority, byte>().AsNoTracking()
                    on category.IdPriority equals priority.Id
                where
                    category.Id == idCategory
                select new DetailedCategoryResponse
                {
                    IdCategory = category.Id,
                    Name = category.Name,
                    Priority = new PriorityResponse { IdPriority = priority.Id, Name = priority.Name }
                }
            );

            return await categoriesQuery.FirstOrDefaultAsync();
        }

        #endregion
    }
}
