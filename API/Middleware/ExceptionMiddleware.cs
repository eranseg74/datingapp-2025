using System.Net;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

// The RequestDelegate is the request delegate we are going to pass the request on to after we checked if there is an error
// The ILogger is a logging system that enables logging to the terminal. The ILogger can support various of logging systems. Tghe looger needs to have as a type, the type of the class we are logging on.
// The IHostEnvironment enables querying what type of environment we are in (dev / prod)
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
  // The name of the method has to be InvokeAsync because we are adding the class as a middleware to the program.cs class, and when we add something to the program class it expects to find this method
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{ex.Message}", ex.Message); // Could write it as 'logger.LogError(ex, ex.Message);' but that will give us a warning. This format behaves exactly the same
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

      var response = env.IsDevelopment() ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace) : new ApiException(context.Response.StatusCode, ex.Message, "Internal server error");

      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      var json = JsonSerializer.Serialize(response, options);

      await context.Response.WriteAsync(json);
    }
  }
}
