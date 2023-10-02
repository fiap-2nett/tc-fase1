using System;

namespace TechChallenge.Application.Contracts.Category
{
    public sealed class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public PriorityResponse Priority { get; set; }
    }
}
