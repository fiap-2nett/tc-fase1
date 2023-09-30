using System;
using System.Xml.Linq;
using TechChallenge.Domain.Core.Abstractions;
using TechChallenge.Domain.Core.Primitives;
using TechChallenge.Domain.Core.Utility;
using TechChallenge.Domain.Enumerations;

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

        public Ticket(int idCompany, int idCategory, int idUserRequester, TicketStatuses status, string description)
        {
            Ensure.GreaterThan(idCompany, 0, "The company identifier must be greater than zero.", nameof(idCompany));
            Ensure.GreaterThan(idCategory, 0, "The category identifier must be greater than zero.", nameof(idCategory));
            Ensure.GreaterThan(idUserRequester, 0, "The status identifier must be greater than zero.", nameof(idUserRequester));
            Ensure.NotEmpty(description, "The ticket description is required.", nameof(description));

            IdCategory = idCategory;
            IdUserRequester = idUserRequester;
            Description = description;
            IdStatus = (byte)status;
        }

        public void AssigneUser(int idTicket, int idAssignedUser)
        {
            Ensure.GreaterThan(idTicket, 0, "The company identifier must be greater than zero.", nameof(idTicket));
            Ensure.GreaterThan(idAssignedUser, 0, "The company identifier must be greater than zero.", nameof(idAssignedUser));

            Id = idTicket;
            IdUserAssigned = idAssignedUser;
        }


        #endregion

        #region Public Methods

        public void AssignTo(int idUserToAssign)
        {
            IdUserAssigned = idUserToAssign;
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

        #endregion
    }
}
