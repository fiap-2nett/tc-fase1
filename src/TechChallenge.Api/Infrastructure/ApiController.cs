using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Api.Constants;
using TechChallenge.Api.Contracts;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Api.Infrastructure
{
    [Authorize]
    public class ApiController : ControllerBase
    {
        #region Methods
        
        protected new IActionResult Ok(object value)
            => base.Ok(value);
        
        protected IActionResult BadRequest(Error error)
            => BadRequest(new ApiErrorResponse(error));
        
        protected new IActionResult NotFound()
            => NotFound(Errors.NotFoudError.Message);

        #endregion
    }
}
