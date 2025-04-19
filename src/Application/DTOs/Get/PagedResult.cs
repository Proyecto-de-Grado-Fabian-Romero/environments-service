namespace EnvironmentsService.Src.Application.DTOs.Get;

public class PagedResult<T>
{
    required public IEnumerable<T> Items { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int Limit { get; set; }

    public int TotalItems { get; set; }
}
