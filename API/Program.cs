using System.Text;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Middleware;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
// Adding the repository as a service so it will be injectable to all other classes
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILikesRepository, LikesRepository>();
builder.Services.AddScoped<LogUserActivity>();
// Cloudinary settings injection. Be careful to match the section name with the appsettings.json file
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

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
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();


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
  options.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200");
});

app.UseAuthentication(); // Asks the question - "Who are you?"
app.UseAuthorization(); // Once we know who they are, what are they allow to do. This means that the order is important. First add the authentication middleware and only after that the authorization middleware. Otherwise it will not work

app.MapControllers();

///// Seeding the data //////
// In the Program class we cannot use dependency injection to get a hold on the DbContext so we will use a different approach which is called 'The service locator pattern':
// Has to be before the app.Run method. Nothing will be executed after that method!
// Creating a scope in which we will run the service. In this case - the AppDbContext - in order to have access to the DB.
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
  var context = services.GetRequiredService<AppDbContext>(); // Getting the AppDbContext service to gain access to the DB
  // Migrating the database in code:
  await context.Database.MigrateAsync(); // The MigrateAsync method applies any pending migrations for the context to the database. If there is no DB, it will be automatically created.
  await Seed.SeedUsers(context);
}
catch (Exception ex)
{
  var logger = services.GetRequiredService<ILogger<Program>>();
  logger.LogError(ex, "An error occured during migration");
}

app.Run();
