using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Api.Constants
{
    internal static class Errors
    {
        internal static Error NotFoudError => new Error(
            "API.NotFoundError",
            "The requested resource was not found.");

        internal static Error ServerError => new Error(
            "API.ServerError",
            "The server encountered an unrecoverable error.");

        internal static Error UnProcessableRequest => new Error(
            "API.UnProcessableRequest",
            "The server could not process the request.");
    }
}
