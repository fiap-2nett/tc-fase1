using System.Collections.Generic;

namespace TechChallenge.Domain.Core.Primitives
{
    public sealed class Error : ValueObject
    {
        #region Properties

        public string Code { get; }
        public string Message { get; }
        internal static Error None => new Error(string.Empty, string.Empty);

        #endregion

        #region Constructors

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        #endregion

        #region Overriden Methods

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Code;
            yield return Message;
        }

        #endregion
    }
}
