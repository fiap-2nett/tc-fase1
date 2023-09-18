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
    }
}
