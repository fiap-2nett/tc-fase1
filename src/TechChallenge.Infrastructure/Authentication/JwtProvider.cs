using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Domain.Entities;
using TechChallenge.Infrastructure.Authentication.Settings;

namespace TechChallenge.Infrastructure.Authentication
{
    internal sealed class JwtProvider : IJwtProvider
    {
        #region Read-Only Fields

        private readonly JwtSettings _jwtSettings;

        #endregion

        #region Constructors

        public JwtProvider(IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        #endregion

        #region IJwtProvider Members

        public string Create(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecurityKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userClaims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name.ToString()),
                new Claim(ClaimTypes.Surname, user.Surname.ToString()),
                new Claim(ClaimTypes.Email, user.Email.ToString())
            };

            var tokenExpirationTime = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes);

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                userClaims,
                null,
                tokenExpirationTime,
                signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}
