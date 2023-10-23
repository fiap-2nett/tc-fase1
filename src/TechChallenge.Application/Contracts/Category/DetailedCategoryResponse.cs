namespace TechChallenge.Application.Contracts.Category
{
    public sealed class DetailedCategoryResponse
    {
        public int IdCategory { get; set; }
        public string Name { get; set; }
        public PriorityResponse Priority { get; set; }
    }
}
