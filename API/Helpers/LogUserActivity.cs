using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
  // The OnActionExecutionAsync method inherited from the IAsyncActionFilter allows us to implement actions after the API call from the controller has been executed. Everything that comes after the await next() is executed after the call to the server was ended and we have the results. The intention here is to update the time a user was active on every action he performs. If we want to execute some actions before the API call we would put the code above the await next() call. The context gives us access to the HttpContext that contains all the data we need - the request and respond
  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    var resultContext = await next();
    // The optional chaining is because we may not have the Identity if the user is not making an authenticated request
    // If the request is not authenticated or the user is not quthenticated we don't want to log anything
    if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
    // Getting the member Id of the user using the extention method we implemented on the User
    var memberId = resultContext.HttpContext.User.GetMemberId();
    // It is not possible to update the DB from the context here so we first get the DB
    var dbContext = resultContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
    // On the Members table - setting the last active property to now
    await dbContext.Members
      .Where(x => x.Id == memberId)
      .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastActive, DateTime.UtcNow));
  }
}
