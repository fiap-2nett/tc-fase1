namespace TechChallenge.Application.Contracts.Tickets
{
    public sealed class TicketRequest
    {
        
        public int IdCompany { get; set; }
        public int IdCategory { get; set; }
        
        public string Description { get; set; }


    }
}
