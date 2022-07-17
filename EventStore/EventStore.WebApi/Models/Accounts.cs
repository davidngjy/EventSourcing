namespace EventStore.WebApi.Models;

public record Account
{
    public Guid AccountId { get; }
    public string Name { get; }
    public decimal AvailableCredit { get; private init; }

    private Account(Guid accountId, string name, decimal availableCredit)
    {
        AccountId = accountId;
        Name = name;
        AvailableCredit = availableCredit;
    }

    public static Account CreateEmpty => new(Guid.Empty, string.Empty, decimal.Zero);

    public Account Apply(object evt) =>
        evt switch
        {
            NewAccount acc => new(acc.Id, acc.Name, 0),
            DebitAccount debit => this with { AvailableCredit = AvailableCredit - debit.Amount },
            CreditAccount credit => this with { AvailableCredit = AvailableCredit + credit.Amount },
            _ => throw new NotImplementedException()
        };
}

public record NewAccountRequest(string Name);

public record NewAccount(Guid Id, string Name);

public record DebitAccount(decimal Amount);

public record CreditAccount(decimal Amount);
