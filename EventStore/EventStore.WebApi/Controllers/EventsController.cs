using System.Text.Json;
using EventStore.Client;
using EventStore.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventStoreClient _eventStoreClient;

    public EventsController(EventStoreClient eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(NewEvent newEvent, CancellationToken cancellationToken)
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            nameof(NewEvent),
            JsonSerializer.SerializeToUtf8Bytes(newEvent));

        var streamName = $"event-stream-{newEvent.Id}";

        await _eventStoreClient.AppendToStreamAsync(
            streamName,
            StreamState.NoStream,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return Created("", new
        {
            StreamName = streamName
        });
    }

    [HttpPost("{streamName}")]
    public async Task<IActionResult> UpdateEvent(
        UpdateEvent updateEvent, string streamName, CancellationToken cancellationToken)
    {
        var eventData = new EventData(
            Uuid.NewUuid(),
            nameof(UpdateEvent),
            JsonSerializer.SerializeToUtf8Bytes(updateEvent));

        await _eventStoreClient.AppendToStreamAsync(
            streamName,
            StreamState.StreamExists,
            new[] { eventData },
            cancellationToken: cancellationToken);

        return Ok();
    }
}
