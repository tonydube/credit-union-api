namespace CreditUnionApi.Models;

public record Member(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime MemberSince
);

public record Account(
    string Id,
    string MemberId,
    string AccountType, // "checking" or "savings"
    decimal Balance
);

public record Transaction(
    string Id,
    string AccountId,
    string Type, // "credit" or "debit"
    decimal Amount,
    string Description,
    DateTime Date
);

public record TransferRequest(
    string FromAccountId,
    string ToAccountId,
    decimal Amount
);

public record LoginRequest(
    string Username,
    string Password
);

public record LoginResponse(
    string Token,
    string MemberId
);
