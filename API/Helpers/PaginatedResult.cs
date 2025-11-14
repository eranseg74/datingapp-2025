using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class PaginatedResult<T>
{
  public PaginationMetadata Metadata { get; set; } = default!;
  public List<T> Items { get; set; } = [];
};

public class PaginationMetadata
{
  public int CurrentPage { get; set; }
  public int TotalPages { get; set; }
  public int PageSize { get; set; }
  public int TotalCount { get; set; }
};

public class PaginationHelper
{
  public static async Task<PaginatedResult<T>> CreateAsync<T>(IQueryable<T> query, int pageNumber, int pageSize)
  {
    // The CountAsync method is an entity framework method that returns the total amount of items in the query result. If the query will be on the Members table it will return how many members are in the table
    var count = await query.CountAsync();
    // The following calculates which items to provide according to the page number and number of items in each page. For example, in page 1 we will skip 0 items ((pageNumber - 1) * pageSize) and take the first 10 elements. In page 2 we will skip 10 items ((2 - 1) * 10) = 10, and take the next 10, and so on...
    // The reference to the database is exeuted by the ToListAsync call.
    var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

    // After the call to the DB we return the metadata along with the result from the DB
    return new PaginatedResult<T>
    {
      Metadata = new PaginationMetadata
      {
        CurrentPage = pageNumber,
        TotalPages = (int)Math.Ceiling(count / (double)pageSize), // if we have 25 items and each page can contain 10 items than 25/10 = 2.5. After ceiling - 3. So we need 3 pages to contain all the elements. I
        PageSize = pageSize,
        TotalCount = count
      },
      Items = items
    };
  }
}