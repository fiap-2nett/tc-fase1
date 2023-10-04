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

        public static class Category
        {
            public const string Get = "categories";
            public const string GetById = "categories/{idCategory:int}";
        }

        public static class Tickets
        {
            public const string Get = "tickets";
            public const string GetById = "tickets/{idTicket:int}";
            public const string Create = "tickets";
            public const string AssignToMe = "tickets/{idTicket:int}/assign-to/me";
            public const string AssignToUser = "tickets/assign-user";
            public const string Complete = "tickets/{idTicket:int}/complete";
            public const string ChangeStatus = "tickets/{idTicket:int}/change-status";
        }

        public static class TicketStatus
        {
            public const string Get = "ticketstatus";
            public const string GetById = "ticketstatus/{idTicketStatus:int}";
        }
    }
}
