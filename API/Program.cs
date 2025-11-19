using System.Text;
using API.Data;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
  opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors();
// When registering a service (such as the TokenService) there are 3 types of services:
// 1) AddSingelton - This means that when the application starts up this will create an instance of the service and keep it running as long as the application is alive. In the Token example that won't be required because once we issued a token the service is no longer needed
// 2) AddTransient - This will create a new instance of the service for every single request. This might be too much because we might use this service several times in the same request
// 3) AddScoped - This will create a new instance of the service once per request
builder.Services.AddScoped<ITokenService, TokenService>(); // Whenever we need this service in the application we will inject the ITokenService interface. According to this line, the app will know that whenever the ITokenService is injected it should use the TokenService (The service that implements the interface)
builder.Services.AddScoped<IPhotoService, PhotoService>();
// No need to add the repositories. They all be handled in the UnitOfWork
/*
// Adding the repository as a service so it will be injectable to all other classes
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILikesRepository, LikesRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
*/
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<LogUserActivity>();
// Cloudinary settings injection. Be careful to match the section name with the appsettings.json file
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Adding the Hub (SignalR) service. After adding this we need to provide it a middleware because we need to foreward all the request that are coning to the hub (see line below the App.MapController).
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>(); // We want the PresenceTracker to start when the application starts up and live as long as the application is on so we use a singleton so it will not get destroyed

// Adding the Identity Framework
// Two more options are the AddIdentityApiEndpoints which provides a built-in ApiController with predefined endpoints. Could be very useful but will probably require adjustments and configurations. The second option is AddIdentity which uses cookies for authentication. Becasue we are using JWT we choose the AddIdentityCore option
builder.Services.AddIdentityCore<AppUser>(opt =>
{
  opt.Password.RequireNonAlphanumeric = false;
  opt.User.RequireUniqueEmail = true; // Using the Identity implementation to check email existance
}).AddRoles<IdentityRole>() // Using the IdentityRole to define roles
  .AddEntityFrameworkStores<AppDbContext>(); // Using EntityFramework to store identities

// Adding the authentication scheme
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
  var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("Token key not found - Program.cs");
  options.TokenValidationParameters = new TokenValidationParameters
  { // Inside we define how we want to validate the token
    ValidateIssuerSigningKey = true, // Making sure that the token is validate when the API server receives it
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
    ValidateIssuer = false, // Who issued the token.
    ValidateAudience = false // Who are suppose to accept the token. Will be true token cannot be forwarded to other sites 
  };

  // When the client first makes a connection through a SignalR Then it sends an HTTP request to establish the connection and see what methods both the client and the server van use, to use real time communication. There are three methods:
  // 1) WebSocket - we use that
  // 2) Long Polling
  // 3) Server-Side Events
  // The initial request will be of type HTTP in order to set up a connection and figure out what connection to use. WebSocket does not send an authentication header but does send the access token as a query string which we will use for all future communication. This is why we are setting the accessToken value. The text must be exactly "access_token" because this is the query property that the function will search to get the user's token. After getting the access token we are setting the Token of the context to that access token and this token will be used in the PresenceHub when we authenticate the user so then we will have an access to the user's claim from which we will get the Email. In order for all of this to work we need to add the AllowCredentials() to the app.UseCors when configuring the HTTP request pipeline (already added since it is also required when using cookies)
  options.Events = new JwtBearerEvents
  {
    OnMessageReceived = context =>
    {
      var accessToken = context.Request.Query["access_token"];
      var path = context.HttpContext.Request.Path;
      if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
      {
        context.Token = accessToken;
      }
      return Task.CompletedTask;
    }
  };
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// Adding policies
builder.Services.AddAuthorizationBuilder()
  .AddPolicy("RequiredAdminRole", policy => policy.RequireRole("Admin"))
  .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));


var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

//app.UseHttpsRedirection(); // Enforce HTTPS redirection for all requests. This means that any HTTP request will be automatically redirected to its HTTPS equivalent.

// The error middleware should be first because if it catches an error it will execute the catch statement and exit the middleware and not pass values to the next middleware so it should be first
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(options =>
{
  options.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200", "https://localhost:4200");
});

app.UseAuthentication(); // Asks the question - "Who are you?"
app.UseAuthorization(); // Once we know who they are, what are they allow to do. This means that the order is important. First add the authentication middleware and only after that the authorization middleware. Otherwise it will not work

app.MapControllers();
// Adding middleware to the hub
app.MapHub<PresenceHub>("hubs/presence"); // All the requests with the following path will be indirected to the hub SignalR
// Next step is to configure an Identity to foreward the authentication header (authorization token) to the presence hub. This is done by adding event options to the AddAuthentication method above
app.MapHub<MessageHub>("hubs/messages"); // Every time we are creating a new hub we have to add it here in this way

///// Seeding the data //////
// In the Program class we cannot use dependency injection to get a hold on the DbContext so we will use a different approach which is called 'The service locator pattern':
// Has to be before the app.Run method. Nothing will be executed after that method!
// Creating a scope in which we will run the service. In this case - the AppDbContext - in order to have access to the DB.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
  var context = services.GetRequiredService<AppDbContext>(); // Getting the AppDbContext service to gain access to the DB
  var userManager = services.GetRequiredService<UserManager<AppUser>>(); // Getting the AppDbContext service to gain access to the DB
  // Migrating the database in code:
  await context.Database.MigrateAsync(); // The MigrateAsync method applies any pending migrations for the context to the database. If there is no DB, it will be automatically created.
  await context.Connections.ExecuteDeleteAsync(); // This will remove all the connections from the database. We want an empty connection list in the database whenever we restart the application so there will be no old connections stuck in the database, whom we cannot reach
  await Seed.SeedUsers(userManager);
}
catch (Exception ex)
{
  var logger = services.GetRequiredService<ILogger<Program>>();
  logger.LogError(ex, "An error occured during migration");
}

app.Run();
