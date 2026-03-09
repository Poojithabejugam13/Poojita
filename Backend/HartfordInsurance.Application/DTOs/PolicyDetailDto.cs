using System.Collections.Generic;
using HartfordInsurance.Domain.Entities;

namespace HartfordInsurance.Application.DTOs;

public class PolicyDetailDto
{
    public CustomerPolicy Policy { get; set; } = null!;
    public List<NomineeDto> Nominees { get; set; } = new();
}
