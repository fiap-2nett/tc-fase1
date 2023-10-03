using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Users;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Api.Controllers
{
    public class UsersController : ApiController
    {
        #region Read-Only Fields

        private readonly IUserService _userService;
        private readonly IUserSessionProvider _userSessionProvider;

        #endregion

        #region Constructors

        public UsersController(IUserService userService, IUserSessionProvider userSessionProvider)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userSessionProvider = userSessionProvider ?? throw new ArgumentNullException(nameof(userSessionProvider));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Represents the query for getting the paged list of the users.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">The page size. The max page size is 100.</param>
        /// <returns>The paged list of the users.</returns>
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(ApiRoutes.Users.Get)]
        [ProducesResponseType(typeof(PagedList<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int page, int pageSize)
        {
            var response = await _userService.GetUsersAsync(new GetUsersRequest(page, pageSize));
            return Ok(response);
        }

        /// <summary>
        /// Represents the query for getting a user authenticated.
        /// </summary>
        /// <returns>The user authenticated info.</returns>
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpGet(ApiRoutes.Users.GetMyProfile)]
        [ProducesResponseType(typeof(DetailedUserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var response = await _userService.GetUserByIdAsync(_userSessionProvider.IdUser);
            return Ok(response);
        }

        /// <summary>
        /// Represents the change password request.
        /// </summary>
        /// <param name="changePasswordRequest">Represents the change password request.</param>
        /// <returns></returns>
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPut(ApiRoutes.Users.ChangePassword)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            await _userService.ChangePasswordAsync(_userSessionProvider.IdUser, changePasswordRequest.Password);
            return Ok();
        }

        /// <summary>
        /// Represents the update user request.
        /// </summary>
        /// <param name="updateUserRequest">Represents the update user request.</param>
        /// <returns></returns>
        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPut(ApiRoutes.Users.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] UpdateUserRequest updateUserRequest)
        {
            await _userService.UpdateUserAsync(_userSessionProvider.IdUser, updateUserRequest.Name, updateUserRequest.Surname);
            return Ok();
        }

        #endregion
    }
}
