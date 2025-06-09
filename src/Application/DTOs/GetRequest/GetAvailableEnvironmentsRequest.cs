namespace EnvironmentsService.Src.Application.DTOs.GetRequest;

public class GetAvailableEnvironmentsRequest
{
    public string? Location { get; set; }

    public string? EnvironmentTypePublicKey { get; set; }

    public long? StartDate { get; set; }

    public long? EndDate { get; set; }

    public List<string>? ServicePublicKeys { get; set; }

    public List<(string AreaPublicKey, int MinQuantity)>? Areas { get; set; }

    public Dictionary<string, int>? EquipmentRequired { get; set; }

    public bool? InstantBookingRequired { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int? MinCapacity { get; set; }
}
