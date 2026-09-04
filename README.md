# Credit Union API

A mock REST API simulating core digital banking operations for a credit union. Built with ASP.NET Core and .NET 10 as a learning project to explore C# backend development patterns.

## Stack

- ASP.NET Core (Minimal API)
- JWT authentication via `Microsoft.AspNetCore.Authentication.JwtBearer`
- OpenAPI/Swagger spec auto-generated at `/openapi/v1.json`

## Endpoints

| Method | Route                                                    | Auth | Description                     |
| ------ | -------------------------------------------------------- | ---- | ------------------------------- |
| POST   | `/auth/login`                                            | No   | Returns a JWT token             |
| GET    | `/api/v1/members/{id}`                                   | Yes  | Get member profile              |
| GET    | `/api/v1/members/{id}/accounts`                          | Yes  | List member accounts            |
| GET    | `/api/v1/members/{id}/accounts/{accountId}`              | Yes  | Get single account with balance |
| GET    | `/api/v1/members/{id}/accounts/{accountId}/transactions` | Yes  | Paginated transaction history   |
| POST   | `/api/v1/members/{id}/accounts/{accountId}/transfer`     | Yes  | Transfer between accounts       |

## Design Decisions

**Versioned routes** — all protected endpoints are prefixed with `/api/v1/` to support future versioning without breaking existing consumers.

**Member-scoped authorization** — every protected endpoint extracts the `memberId` claim from the JWT and verifies it matches the requested resource. A member can only access their own data.

**Pagination on transactions** — transaction history supports `page` and `pageSize` query params, defaulting to page 1 with 10 results, ordered by most recent first.

**Balance validation on transfer** — transfers check for sufficient funds and return a `400 Bad Request` with a clear message rather than allowing negative balances.

## Running Locally

```bash
dotnet watch
```

Login with either test account (password is `password` for both):

- `jane.doe@email.com`
- `john.smith@email.com`

API reference UI available at http://localhost:{PORT}/scalar/v1
