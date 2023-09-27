using System;

namespace TechChallenge.Application.Contracts.Tickets;

public sealed class DetailedTicketResponse
{
    public int IdTicked { get; set; }
    public string Description { get; set; }
    public CategoryReponse Category { get; set; }
    public StatusResponse Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public int? LastUpdatedBy { get; set; }
    public string CancellationReason { get; set; }
}
