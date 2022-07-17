namespace EventStore.WebApi.Models;

public record NewEvent(int Id, string Message);

public record UpdateEvent(string Message);
