using System;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Core.Utility;
using TechChallenge.Domain.Core.Primitives;
using TechChallenge.Domain.Core.Abstractions;

namespace TechChallenge.Domain.Entities
{
    public sealed class Ticket : AggregateRoot<int>, IAuditableEntity, ISoftDeletableEntity
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

        public Ticket(int idCategory, int idUserRequester, string description)
        {
            Ensure.GreaterThan(idCategory, 0, "The category identifier must be greater than zero.", nameof(idCategory));
            Ensure.GreaterThan(idUserRequester, 0, "The user requester identifier must be greater than zero.", nameof(idUserRequester));
            Ensure.NotEmpty(description, "The ticket description is required.", nameof(description));

            IdCategory = idCategory;
            IdUserRequester = idUserRequester;
            Description = description;
            IdStatus = (byte)TicketStatuses.New;
        }

        #endregion

        #region Public Methods

        public void AssignTo(int idUserAssigned)
        {
            Ensure.GreaterThan(idUserAssigned, 0, "The user assigned identifier must be greater than zero.", nameof(idUserAssigned));
            IdUserAssigned = idUserAssigned;
        }

        public void Cancel(string cancellationReason)
        {
            Ensure.NotEmpty(cancellationReason, "The cancellation reason is required.", nameof(cancellationReason));
            CancellationReason = cancellationReason;
        }

        public void Complete()
        {
            CompletedAt = DateTime.Now;
            IdStatus = (byte)TicketStatuses.Completed;
        }

        public void ChangeStatus(int ticketStatus)
        {
            Ensure.GreaterThan(ticketStatus, 0, "The status informed must be greater than zero.", nameof(ticketStatus));
            LastUpdatedAt = DateTime.Now;
            IdStatus = (byte)ticketStatus;
        }

        #endregion
    }
}
