using System;
using System.Linq;
using System.Collections.Generic;
using TechChallenge.Domain.Errors;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Domain.ValueObjects
{
    public sealed class Password : ValueObject
    {
        #region Constants

        private const int MinPasswordLength = 6;

        #endregion

        #region Read-Only Fields

        private static readonly Func<char, bool> IsLower = c => c >= 'a' && c <= 'z';
        private static readonly Func<char, bool> IsUpper = c => c >= 'A' && c <= 'Z';
        private static readonly Func<char, bool> IsDigit = c => c >= '0' && c <= '9';
        private static readonly Func<char, bool> IsNonAlphaNumeric = c => !(IsLower(c) || IsUpper(c) || IsDigit(c));

        #endregion

        #region Properties

        public string Value { get; }

        #endregion

        #region Constructors

        private Password(string value) => Value = value;

        #endregion

        #region Factory Methods

        public static Password Create(string password)
        {
            if (password.IsNullOrWhiteSpace())
                throw new ArgumentException(DomainErrors.Password.NullOrEmpty.Message, nameof(password));

            if (password.Length < MinPasswordLength)
                throw new ArgumentException(DomainErrors.Password.TooShort.Message, nameof(password));

            if (!password.ToCharArray().Any(IsLower))
                throw new ArgumentException(DomainErrors.Password.MissingLowercaseLetter.Message, nameof(password));

            if (!password.ToCharArray().Any(IsUpper))
                throw new ArgumentException(DomainErrors.Password.MissingUppercaseLetter.Message, nameof(password));

            if (!password.ToCharArray().Any(IsDigit))
                throw new ArgumentException(DomainErrors.Password.MissingDigit.Message, nameof(password));

            if (!password.ToCharArray().Any(IsNonAlphaNumeric))
                throw new ArgumentException(DomainErrors.Password.MissingNonAlphaNumeric.Message, nameof(password));

            return new Password(password);
        }

        #endregion

        #region Overriden Methods

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        #endregion

        #region Operators

        public static implicit operator string(Password password)
            => password?.Value ?? string.Empty;

        #endregion
    }
}
