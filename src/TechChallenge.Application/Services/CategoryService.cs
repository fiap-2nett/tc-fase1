using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Contracts.Category;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Domain.Entities;

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

        public async Task<IEnumerable<DetailedCategoryResponse>> GetAsync()
        {
            IQueryable<DetailedCategoryResponse> categoriesQuery = (
                from category in _dbContext.Set<Category, int>().AsNoTracking()
                join priority in _dbContext.Set<Priority, byte>().AsNoTracking()
                    on category.IdPriority equals priority.Id
                select new DetailedCategoryResponse
                {
                    Id= category.Id,
                    Name= category.Name,
                    CreatedAt= category.CreatedAt,
                    Priority = new PriorityResponse { Id = priority.Id, Name = priority.Name }
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
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt,
                    Priority = new PriorityResponse { Id = priority.Id, Name = priority.Name }
                }
            );

            return await categoriesQuery.FirstOrDefaultAsync();
        }

        #endregion
    }
}
