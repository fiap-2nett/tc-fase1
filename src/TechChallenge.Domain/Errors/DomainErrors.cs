using TechChallenge.Domain.Core.Primitives;

namespace TechChallenge.Domain.Errors
{
    public static class DomainErrors
    {
        public static class General
        {
            public static Error UnProcessableRequest => new Error(
                "General.UnProcessableRequest",
                "The server could not process the request.");

            public static Error ServerError => new Error(
                "General.ServerError",
                "The server encountered an unrecoverable error.");
        }

        public static class Authentication
        {
            public static Error InvalidEmailOrPassword => new Error(
                "Authentication.InvalidEmailOrPassword",
                "The specified email or password are incorrect.");
        }

        public static class Email
        {
            public static Error NullOrEmpty => new Error(
                "Email.NullOrEmpty",
                "The email is required.");

            public static Error LongerThanAllowed => new Error(
                "Email.LongerThanAllowed",
                "The email is longer than allowed.");

            public static Error InvalidFormat => new Error(
                "Email.InvalidFormat",
                "The email format is invalid.");
        }

        public static class Password
        {
            public static Error NullOrEmpty => new Error(
                "Password.NullOrEmpty",
                "The password is required.");

            public static Error TooShort => new Error(
                "Password.TooShort",
                "The password is too short.");

            public static Error MissingUppercaseLetter => new Error(
                "Password.MissingUppercaseLetter",
                "The password requires at least one uppercase letter.");

            public static Error MissingLowercaseLetter => new Error(
                "Password.MissingLowercaseLetter",
                "The password requires at least one lowercase letter.");

            public static Error MissingDigit => new Error(
                "Password.MissingDigit",
                "The password requires at least one digit.");

            public static Error MissingNonAlphaNumeric => new Error(
                "Password.MissingNonAlphaNumeric",
                "The password requires at least one non-alphanumeric.");
        }

        public static class User
        {
            public static Error NotFound => new Error(
                "User.NotFound",
                "The user with the specified identifier was not found.");

            public static Error InvalidPermissions => new Error(
                "User.InvalidPermissions",
                "The current user does not have the permissions to perform that operation.");

            public static Error DuplicateEmail => new Error(
                "User.DuplicateEmail",
                "The specified email is already in use.");

            public static Error CannotChangePassword => new Error(
                "User.CannotChangePassword",
                "The password cannot be changed to the specified password.");
        }

        public static class Ticket
        {
            public static Error NotFound => new Error(
                "Ticket.NotFound",
                "The ticket with the specified identifier was not found.");

            public static Error CannotBeCompletedByThisUser => new Error(
                "Ticket.CannotBeCompletedByThisUser",
                "The ticket cannot be completed by this user.");

            public static Error HasNotBeenAssignedToAUser => new Error(
                "Ticket.HasNotBeenAssignedToAUser",
                "The ticket has not been assigned to a user.");

            public static Error HasAlreadyBeenCompletedOrCancelled => new Error(
                "Ticket.HasAlreadyBeenCompletedOrCancelled",
                "This ticket has already been completed or cancelled.");

            public static Error StatusCannotBeChangedByThisUser => new Error(
                "Ticket.StatusCannotBeChangedByThisUser",
                "The ticket status cannot be changed by this user.");

            public static Error CannotChangeStatusToNew => new Error(
                "Ticket.CannotChangeStatusToNew",
                "Change ticket status to 'New' is not permitted.");

            public static Error StatusNotAllowed => new Error(
                "Ticket.StatusNotAllowed",
                "The ticket status cannot be changed to the one informed.");

            public static Error StatusDoesNotExist => new Error(
                "Ticket.StatusNotAllowed",
                "The informed status is not valid or does not exist.");

            public static Error CannotBeAssignedToThisUser = new Error(
                "Ticket.CantBeAssignedToThisUser",
                "The ticket cannot be assigned to this user.");

            public static Error CancellationReasonIsRequired = new Error(
                "Ticket.CancellationReasonIsRequired",
                "The reason for ticket cancellation is required.");
        }
    }
}
