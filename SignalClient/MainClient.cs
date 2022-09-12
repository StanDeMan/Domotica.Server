using Microsoft.AspNetCore.SignalR.Client;

namespace SignalClient;

public class MainClient
{
    public static async Task ExecuteAsync()
    {
        var uri = "http://192.168.2.46:7078/current-time";

        await using var connection = new HubConnectionBuilder().WithUrl(uri).Build();

        await connection.StartAsync();

        await foreach (var date in connection.StreamAsync<DateTime>("Streaming"))
        {
            Console.WriteLine(date);
        }
    }
}