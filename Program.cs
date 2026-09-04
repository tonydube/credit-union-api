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

// GET /api/v1/members/{id}
app.MapGet("/api/v1/members/{id}", (string id, ClaimsPrincipal user) =>
{
    var memberId = user.FindFirst("memberId")?.Value;
    if (memberId != id) return Results.Forbid();

    var member = SeedData.Members.FirstOrDefault(m => m.Id == id);
    return member is null ? Results.NotFound() : Results.Ok(member);
}).RequireAuthorization();

// GET /api/v1/members/{id}/accounts
app.MapGet("/api/v1/members/{id}/accounts", (string id, ClaimsPrincipal user) =>
{
    var memberId = user.FindFirst("memberId")?.Value;
    if (memberId != id) return Results.Forbid();

    var accounts = SeedData.Accounts.Where(a => a.MemberId == id).ToList();
    return Results.Ok(accounts);
}).RequireAuthorization();

// GET /api/v1/members/{id}/accounts/{accountId}
app.MapGet("/api/v1/members/{id}/accounts/{accountId}", (string id, string accountId, ClaimsPrincipal user) =>
{
    var memberId = user.FindFirst("memberId")?.Value;
    if (memberId != id) return Results.Forbid();

    var account = SeedData.Accounts.FirstOrDefault(a => a.Id == accountId && a.MemberId == id);
    return account is null ? Results.NotFound() : Results.Ok(account);
}).RequireAuthorization();

// GET /api/v1/members/{id}/accounts/{accountId}/transactions
app.MapGet("/api/v1/members/{id}/accounts/{accountId}/transactions", (string id, string accountId, ClaimsPrincipal user, int page = 1, int pageSize = 10) =>
{
    var memberId = user.FindFirst("memberId")?.Value;
    if (memberId != id) return Results.Forbid();

    var account = SeedData.Accounts.FirstOrDefault(a => a.Id == accountId && a.MemberId == id);
    if (account is null) return Results.NotFound();

    var transactions = SeedData.Transactions
        .Where(t => t.AccountId == accountId)
        .OrderByDescending(t => t.Date)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Results.Ok(new { page, pageSize, total = transactions.Count, transactions });
}).RequireAuthorization();

// POST /api/v1/members/{id}/accounts/{accountId}/transfer
app.MapPost("/api/v1/members/{id}/accounts/{accountId}/transfer", (string id, string accountId, TransferRequest req, ClaimsPrincipal user) =>
{
    var memberId = user.FindFirst("memberId")?.Value;
    if (memberId != id) return Results.Forbid();

    var fromAccount = SeedData.Accounts.FirstOrDefault(a => a.Id == req.FromAccountId && a.MemberId == id);
    var toAccount = SeedData.Accounts.FirstOrDefault(a => a.Id == req.ToAccountId && a.MemberId == id);

    if (fromAccount is null || toAccount is null) return Results.NotFound();
    if (fromAccount.Balance < req.Amount) return Results.BadRequest("Insufficient funds.");

    SeedData.Accounts[SeedData.Accounts.IndexOf(fromAccount)] = fromAccount with { Balance = fromAccount.Balance - req.Amount };
    SeedData.Accounts[SeedData.Accounts.IndexOf(toAccount)] = toAccount with { Balance = toAccount.Balance + req.Amount };

    return Results.Ok(new { message = "Transfer successful.", amount = req.Amount });
}).RequireAuthorization();

app.Run();
