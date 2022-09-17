using Microsoft.AspNetCore.SignalR.Client;

namespace SignalClient;

public static class MainClient
{
    public static async Task ExecuteAsync()
    {
        var uri = "http://localhost:7078/current-time";

        await using var connection = new HubConnectionBuilder().WithUrl(uri).Build();

        await connection.StartAsync();

        await foreach (var date in connection.StreamAsync<DateTime>("Streaming"))
        {
            Console.WriteLine(date);
        }
    }
}