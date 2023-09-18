using System;
using System.Collections.Generic;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Persistence.Core.Primitives;

namespace TechChallenge.Persistence.Seeds
{
    internal sealed class TicketStatusSeed : EntitySeedConfiguration<TicketStatus, byte>
    {
        public override IEnumerable<object> Seed()
        {
            yield return new { Id = (byte)TicketStatuses.New,           Name = "Novo",          IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)TicketStatuses.Assigned,      Name = "Atribuído",     IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)TicketStatuses.InProgress,    Name = "Em andamento",  IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)TicketStatuses.OnHold,        Name = "Em espera",     IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)TicketStatuses.Completed,     Name = "Concluído",     IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)TicketStatuses.Canceled,      Name = "Cancelado",     IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
        }
    }
}
