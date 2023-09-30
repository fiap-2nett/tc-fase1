using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Application.Services;

namespace TechChallenge.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITicketService, TicketService>();

            return services;
        }
    }
}
