using System;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Domain.Exceptions
{
    public class InvalidPermissionException : Exception
    {
        public Error Error { get; }

        public InvalidPermissionException(Error error) : base(error.Message)
            => Error = error;
    }
}
