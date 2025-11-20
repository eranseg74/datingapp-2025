using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

// The purpose of this controller is to handle all the requests that are not handled by any other controller. This is useful when we are hosting an Angular application inside the API project and we want to make sure that all the requests that are not handled by the API controllers will be forwarded to the Angular application which will handle the routing on the client side
public class FallbackController : Controller // Inherit from Controller in order to have access to the View method. In other words this can return an HTML content
{
  public IActionResult Index()
  {
    return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML"); // This will return the index.html file located in the wwwroot folder. The PhysicalFile method is used to return a physical file from the file system. The first parameter is the path to the file and the second parameter is the content type. All the other routes that are not handled by any other controller will be forwarded to this method which will return the index.html file. From there Angular will take over and handle the routing on the client side
  }
}
