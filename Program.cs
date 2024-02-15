using JwtAspNet;
using JwtAspNet.Models;
using JwtAspNet.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<TokenService>();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = 
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.PrivateKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("admin", p => p.RequireRole("admin"));
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", (TokenService service) => service.Create(
    new User
    (1,
    "Ana Flávia",
    "anaflavia@email.com",
    "https://github.com/anamandello/image",
    "12345",
    new[] { "admin", "client", "mod" }
    )));

app.MapGet("/restrito", (ClaimsPrincipal user) => new
    {
        id = user.Claims.FirstOrDefault(x => x.Type == "id").ToString(),
        name = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).ToString(),
        email = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).ToString(),
        givenName = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName).ToString(),
        image = user.Claims.FirstOrDefault(x => x.Type == "image").ToString(),

}
).RequireAuthorization();
app.MapGet("/admin", () => "Acesso autorizado").RequireAuthorization("admin");

app.Run();
