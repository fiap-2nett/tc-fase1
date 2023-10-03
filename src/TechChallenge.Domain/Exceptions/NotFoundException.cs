using System;
using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public Error Error { get; }

        public NotFoundException(Error error) : base(error.Message)
            => Error = error;
    }
}
