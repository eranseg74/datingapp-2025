namespace API.Helpers;

public class PagingParams
{
  private const int MaxPageSize = 50;
  public int PageNumber { get; set; } = 1; // Must start with 1 because 0 will give a -1 value in the page calculation (see PagingResult class)
  private int _pageSize = 10;
  public int PageSize
  {
    get => _pageSize;
    set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
  }
}
