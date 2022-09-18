using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace SignalDeviceClient;

public static class MainClient
{
    public static async Task ExecuteAsync()
    {
        const string uri = "https://localhost:7077/device";

        await using var connection = new HubConnectionBuilder()
            .WithUrl(uri)
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync();

        var device = new
        {
            DeviceId = "TIxnxU38dzkm",
            Name = "RGB Led Stripe",
            NameId = "RGBLedStripe",
            Params = new
            {
                Type = "LedStripe",
                Command = "p 3 0 p 4 0 p 14 0",
                LastValue = 100,
                Brightness = 0
            }
        };

        var jsonDevice = JsonConvert.SerializeObject(device);

        await connection.InvokeAsync("GetDeviceStatusInitial", jsonDevice, device.NameId);

        for (var i = 0; i < 3; i++)
        {
            await connection.InvokeAsync("DeviceStatusSend", jsonDevice, device.NameId);
            await connection.InvokeAsync("SendCommand", jsonDevice);
            await Task.Delay(1000);
        }
    }
}
