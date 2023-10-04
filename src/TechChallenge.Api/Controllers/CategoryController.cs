using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Api.Controllers
{
    public sealed class CategoryController : ApiController
    {
        #region Read-Only Fields

        private readonly ICategoryService _categoryService;

        #endregion

        #region Constructors

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Represents the query for getting the list of the categories.
        /// </summary>
        /// <returns>The list of the categories</returns>
        [HttpGet(ApiRoutes.Category.Get)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
            => Ok(await _categoryService.GetAsync());

        /// <summary>
        /// Represents the query to get the category by id.
        /// </summary>
        /// <param name="idCategory"></param>
        /// <returns>One category</returns>
        [HttpGet(ApiRoutes.Category.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] int idCategory)
        {
            var response = await _categoryService.GetByIdAsync(idCategory);
            if (response is null) return NotFound();

            return Ok(response);
        }

        #endregion
    }
}
