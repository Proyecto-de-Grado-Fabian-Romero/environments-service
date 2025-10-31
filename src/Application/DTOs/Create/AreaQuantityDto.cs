using System;

namespace EnvironmentsService.Src.Application.DTOs.Create;

public class AreaQuantityDto
{
    required public string AreaPublicKey { get; set; }

    public int Quantity { get; set; }
}