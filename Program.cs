using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CreditUnionApi.Data;
using CreditUnionApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtKey = "this-is-a-dev-secret-key-change-in-production";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// POST /auth/login
app.MapPost("/auth/login", (LoginRequest req) =>
{
    if (!SeedData.Users.TryGetValue(req.Username, out var memberId) || req.Password != "password")
        return Results.Unauthorized();

    var claims = new[] { new Claim("memberId", memberId) };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);

    return Results.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), memberId));
});

app.Run();
