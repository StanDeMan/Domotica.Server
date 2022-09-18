using Microsoft.AspNetCore.SignalR.Client;

namespace SignalStreamingTimeClient;

public static class MainClient
{
    public static async Task ExecuteAsync()
    {
        var uri = "https://localhost:7077/current-time";

        await using var connection = new HubConnectionBuilder().WithUrl(uri).Build();

        await connection.StartAsync();

        await foreach (var date in connection.StreamAsync<DateTime>("Streaming"))
        {
            Console.WriteLine(date);
        }
    }
}