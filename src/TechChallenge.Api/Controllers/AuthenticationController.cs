using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using TechChallenge.Application.Contracts.Authentication;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Api.Controllers
{
    [AllowAnonymous]
    public sealed class AuthenticationController : ApiController
    {
        #region Read-Only Fields

        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        #endregion

        #region Constructors

        public AuthenticationController(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        #endregion

        #region Endpoints

        [HttpPost(ApiRoutes.Authentication.Login)]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _authenticationService.Login(loginRequest.Email, loginRequest.Password);
            return Ok(response);
        }

        [HttpPost(ApiRoutes.Authentication.Register)]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RegisterRequest registerRequest)
        {
            var response = await _userService.CreateAsync(registerRequest.Name, registerRequest.Surname, registerRequest.Email, registerRequest.Password);
            return Ok(response);
        }

        #endregion
    }
}
