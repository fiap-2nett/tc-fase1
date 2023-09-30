using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechChallenge.Application.Contracts.Tickets;

public sealed class TicketResponse
{
    public int IdTicked { get; set; }
    public string Description { get; set; }
    public CategoryReponse Category { get; set; }
    public StatusResponse Status { get; set; }
    public int IdUserRequester { get; set; }
    public int IdUserAssigned { get; set; }

}
