using System.Text.Json;
using EventStore.Client;

namespace EventStore.WebApi.Services;

public static class DeserializerService
{
    public static object Deserialize(this ResolvedEvent resolvedEvent) =>
        JsonSerializer
            .Deserialize(resolvedEvent.Event.Data.Span, GetType(resolvedEvent.Event.EventType)) ?? throw new Exception("Should not happen");

    private static Type GetType(string typeName) =>
        AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes().Where(y => y.FullName == typeName || y.Name == typeName))
            .First();
}
