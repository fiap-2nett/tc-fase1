using TechChallenge.Domain.ValueObjects;

namespace TechChallenge.Application.Core.Abstractions.Cryptography
{
    public interface IPasswordHasher
    {        
        string HashPassword(Password password);
    }
}
