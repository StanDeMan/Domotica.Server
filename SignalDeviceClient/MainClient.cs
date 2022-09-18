using Microsoft.AspNetCore.SignalR.Client;

namespace SignalDeviceClient;

public static class MainClient
{
    public static async Task ExecuteAsync()
    {
        var uri = "https://localhost:7077/device";

        await using var connection = new HubConnectionBuilder().WithUrl(uri).Build();

        await connection.StartAsync();

    }
}
