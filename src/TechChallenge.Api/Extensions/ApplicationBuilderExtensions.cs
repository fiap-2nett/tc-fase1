using Microsoft.AspNetCore.Builder;
using TechChallenge.Api.Middlewares;

namespace TechChallenge.Api.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        #region Extension Methods

        internal static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder builder)
        {
            builder.UseSwagger();
            builder.UseSwaggerUI(swaggerUiOptions => swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", "TechChallenge API"));

            return builder;
        }

        internal static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
            => builder.UseMiddleware<ExceptionHandlerMiddleware>();

        #endregion
    }
}
