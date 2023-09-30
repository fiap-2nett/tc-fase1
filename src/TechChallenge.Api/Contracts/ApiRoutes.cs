namespace TechChallenge.Api.Contracts
{
    public static class ApiRoutes
    {
        public static class Authentication
        {
            public const string Login = "authentication/login";
            public const string Register = "authentication/register";
        }

        public static class Users
        {
            public const string Get = "users";
            public const string GetMyProfile = "users/me";            
            public const string Update = "users/me";
            public const string ChangePassword = "users/me/change-password";
        }

        public static class Tickets
        {
            public const string GetAllTickets = "tickets/get/all";
            public const string GetByIdTickets = "tickets/get/by-id-detailed";
            public const string CreateTicket = "tickets/create";
            public const string AssigneTicket = "tickets/assigne";

        }
    }
}
