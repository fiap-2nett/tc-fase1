using System;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public Error Error { get; }

        public DomainException(Error error) : base(error.Message)
            => Error = error;
    }
}
