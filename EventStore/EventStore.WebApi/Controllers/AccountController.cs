using System.Text.Json;
using EventStore.Client;
using EventStore.WebApi.Models;
using EventStore.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly EventStoreClient _eventStoreClient;

    public AccountController(EventStoreClient eventStoreClient) => _eventStoreClient = eventStoreClient;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(Guid id, CancellationToken cancellationToken)
    {
        var events = _eventStoreClient.ReadStreamAsync(
            Direction.Forwards,
            $"account-{id}",
            StreamPosition.Start,
            cancellationToken: cancellationToken);

        var finalAccount = await events.AggregateAsync(
            Account.CreateEmpty,
            (account, evt) => account.Apply(evt.Deserialize()),
            cancellationToken);

        return Ok(finalAccount);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(NewAccountRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var newAccount = new NewAccount(id, request.Name);

        var data = new EventData(
            Uuid.NewUuid(),
            nameof(NewAccount),
            JsonSerializer.SerializeToUtf8Bytes(newAccount));

        await _eventStoreClient.AppendToStreamAsync(
            $"account-{id}",
            StreamState.NoStream,
            new[] { data },
            cancellationToken: cancellationToken);

        return CreatedAtAction(nameof(GetAccount), new { id = id }, id);
    }

    [HttpPost("{id}/debit")]
    public async Task<IActionResult> DebitFromAccount(Guid id, DebitAccount debit, CancellationToken cancellationToken)
    {
        var data = new EventData(
            Uuid.NewUuid(),
            nameof(DebitAccount),
            JsonSerializer.SerializeToUtf8Bytes(debit));

        await _eventStoreClient.AppendToStreamAsync(
            $"account-{id}",
            StreamState.StreamExists,
            new[] { data },
            cancellationToken: cancellationToken);

        return Ok();
    }

    [HttpPost("{id}/credit")]
    public async Task<IActionResult> CreditToAccount(Guid id, CreditAccount credit, CancellationToken cancellationToken)
    {
        var data = new EventData(
            Uuid.NewUuid(),
            nameof(CreditAccount),
            JsonSerializer.SerializeToUtf8Bytes(credit));

        await _eventStoreClient.AppendToStreamAsync(
            $"account-{id}",
            StreamState.StreamExists,
            new[] { data },
            cancellationToken: cancellationToken);

        return Ok();
    }
}
