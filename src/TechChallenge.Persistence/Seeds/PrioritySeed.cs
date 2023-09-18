using System;
using System.Collections.Generic;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Persistence.Core.Primitives;

namespace TechChallenge.Persistence.Seeds
{
    internal sealed class PrioritySeed : EntitySeedConfiguration<Priority, byte>
    {
        public override IEnumerable<object> Seed()
        {
            yield return new { Id = (byte)Priorities.Low,       Name = "Baixa",     Sla = 48,   IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)Priorities.Medium,    Name = "Média",     Sla = 24,   IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)Priorities.High,      Name = "Alta",      Sla = 8,    IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
            yield return new { Id = (byte)Priorities.Criticial, Name = "Crítico",   Sla = 4,    IsDeleted = false, CreatedAt = DateTime.MinValue.Date };
        }
    }
}
