using System;
using TechChallenge.Domain.Errors;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Core.Primitives;
using TechChallenge.Domain.Core.Abstractions;
using TechChallenge.Domain.Extensions;

namespace TechChallenge.Domain.Entities
{
    public class Ticket : AggregateRoot<int>, IAuditableEntity, ISoftDeletableEntity
    {
        #region Properties

        public int IdCategory { get; private set; }
        public byte IdStatus { get; private set; }

        public int IdUserRequester { get; private set; }
        public int? IdUserAssigned { get; private set; }

        public string Description { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string CancellationReason { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public int? LastUpdatedBy { get; private set; }
        public DateTime? LastUpdatedAt { get; private set; }

        #endregion

        #region Constructors

        private Ticket()
        { }

        public Ticket(Category category, string description, User userRequester)
        {
            if (category is null)
                throw new DomainException(DomainErrors.Category.NotFound);

            if (userRequester is null)
                throw new DomainException(DomainErrors.User.NotFound);

            if (description.IsNullOrWhiteSpace())
                throw new DomainException(DomainErrors.Ticket.DescriptionIsRequired);

            IdCategory = category.Id;
            Description = description;
            IdUserRequester = userRequester.Id;
            IdStatus = (byte)TicketStatuses.New;
        }

        #endregion

        #region Public Methods

        public void Update(Category category, string description, User userPerformedAction)
        {
            if (category is null)
                throw new DomainException(DomainErrors.Category.NotFound);

            if (description.IsNullOrWhiteSpace())
                throw new DomainException(DomainErrors.Ticket.DescriptionIsRequired);

            if (userPerformedAction is null)
                throw new DomainException(DomainErrors.User.NotFound);

            if (IdUserRequester != userPerformedAction.Id)
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);

            if (IdStatus == (byte)TicketStatuses.Completed || IdStatus == (byte)TicketStatuses.Cancelled)
                throw new InvalidPermissionException(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);

            IdCategory = category.Id;
            Description = description;
            LastUpdatedBy = userPerformedAction.Id;
        }

        public void AssignTo(User userAssigned, User userPerformedAction)
        {
            if (userAssigned is null || userPerformedAction is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            if (userAssigned.IdRole != (byte)UserRoles.Analyst)
                throw new InvalidPermissionException(DomainErrors.Ticket.CannotBeAssignedToThisUser);

            if (userPerformedAction.IdRole == (byte)UserRoles.General)
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);

            if (IdStatus == (byte)TicketStatuses.Completed || IdStatus == (byte)TicketStatuses.Cancelled)
                throw new InvalidPermissionException(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);

            if (IdStatus == (byte)TicketStatuses.New && userPerformedAction.Id != userAssigned.Id && userPerformedAction.IdRole != (byte)UserRoles.Administrator)
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);

            if (IdUserAssigned != userPerformedAction.Id && userPerformedAction.IdRole != (byte)UserRoles.Administrator && IdStatus != (byte)TicketStatuses.New)
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);

            IdUserAssigned = userAssigned.Id;
            LastUpdatedBy = userPerformedAction.Id;
            IdStatus = (byte)TicketStatuses.Assigned;
        }

        public void Cancel(string cancellationReason, User userPerformedAction)
        {
            if (userPerformedAction is null)
                throw new DomainException(DomainErrors.User.NotFound);

            if (userPerformedAction.IdRole == (byte)UserRoles.General && IdUserRequester != userPerformedAction.Id)
                throw new InvalidPermissionException(DomainErrors.Ticket.CannotBeCancelledByThisUser);

            if (userPerformedAction.IdRole == (byte)UserRoles.Analyst && (IdUserAssigned != userPerformedAction.Id && IdUserRequester != userPerformedAction.Id))
                throw new InvalidPermissionException(DomainErrors.Ticket.CannotBeCancelledByThisUser);

            if (cancellationReason.IsNullOrWhiteSpace())
                throw new DomainException(DomainErrors.Ticket.CancellationReasonIsRequired);

            if (IdStatus == (byte)TicketStatuses.Completed || IdStatus == (byte)TicketStatuses.Cancelled)
                throw new InvalidPermissionException(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);

            CancellationReason = cancellationReason;
            LastUpdatedBy = userPerformedAction.Id;
            IdStatus = (byte)TicketStatuses.Cancelled;
        }

        public void Complete(User userPerformedAction)
        {
            if (userPerformedAction is null)
                throw new DomainException(DomainErrors.User.NotFound);

            if (userPerformedAction.IdRole == (byte)UserRoles.General)
                throw new InvalidPermissionException(DomainErrors.Ticket.CannotBeCompletedByThisUser);

            if (userPerformedAction.IdRole == (byte)UserRoles.Analyst && IdUserAssigned != userPerformedAction.Id)
                throw new InvalidPermissionException(DomainErrors.Ticket.CannotBeCompletedByThisUser);

            if (IdStatus == (byte)TicketStatuses.New)
                throw new DomainException(DomainErrors.Ticket.HasNotBeenAssignedToAUser);

            if (IdStatus == (byte)TicketStatuses.Completed || IdStatus == (byte)TicketStatuses.Cancelled)
                throw new DomainException(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);

            CompletedAt = DateTime.Now;
            LastUpdatedBy = userPerformedAction.Id;
            IdStatus = (byte)TicketStatuses.Completed;
        }

        public void ChangeStatus(TicketStatuses changedStatus, User userPerformedAction)
        {
            if (userPerformedAction is null)
                throw new DomainException(DomainErrors.User.NotFound);

            if (userPerformedAction.IdRole == (byte)UserRoles.General || (userPerformedAction.IdRole == (byte)UserRoles.Analyst && IdUserAssigned != userPerformedAction.Id))
                throw new InvalidPermissionException(DomainErrors.Ticket.StatusCannotBeChangedByThisUser);

            if (changedStatus == TicketStatuses.New)
                throw new DomainException(DomainErrors.Ticket.CannotChangeStatusToNew);

            if (IdStatus == (byte)TicketStatuses.Cancelled || IdStatus == (byte)TicketStatuses.Completed)
                throw new DomainException(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);

            if (changedStatus != TicketStatuses.InProgress && changedStatus != TicketStatuses.OnHold && changedStatus != TicketStatuses.Completed)
                throw new DomainException(DomainErrors.Ticket.StatusNotAllowed);

            IdStatus = (byte)changedStatus;
            LastUpdatedBy = userPerformedAction.Id;
        }

        #endregion
    }
}
