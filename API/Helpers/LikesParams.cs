namespace API.Helpers;

public class LikesParams : PagingParams
{
  public string Predicate { get; set; } = "liked";
  public string MemberId { get; set; } = "";
}
