using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Acheve.TestHost;
using Microsoft.AspNetCore.TestHost;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Api.IntegrationTests.Extensions
{
    internal static class RequestBuilderExtensions
    {
        #region Extension Methods

        public static RequestBuilder WithIdentity(this RequestBuilder requestBuilder, User userProfile)
        {
            return requestBuilder.WithIdentity(
                BuildClaims(userProfile),
                TestServerDefaults.AuthenticationScheme);
        }

        public static Task<HttpResponseMessage> PutAsync(this RequestBuilder request)
        {            
            ArgumentNullException.ThrowIfNull(request, nameof(request));  
            return request.SendAsync(HttpMethod.Put.ToString());
        }

        public static Task<HttpResponseMessage> PatchAsync(this RequestBuilder request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            return request.SendAsync(HttpMethod.Patch.ToString());
        }

        public static Task<HttpResponseMessage> DeleteAsync(this RequestBuilder request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            return request.SendAsync(HttpMethod.Delete.ToString());
        }

        #endregion

        #region Private Methods

        private static IEnumerable<Claim> BuildClaims(User user)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            yield return new Claim(ClaimTypes.Name, user.Name);
            yield return new Claim(ClaimTypes.Surname, user.Surname);
            yield return new Claim(ClaimTypes.Email, user.Email);
        }

        #endregion
    }
}
