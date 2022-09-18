using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;

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

        for (var i = 0; i < 5; i++)
        {
            await connection.InvokeAsync("DeviceStatusSend", jsonDevice, device.NameId);
            await connection.InvokeAsync("SendCommand", jsonDevice);
            await Task.Delay(1000);

            jsonDevice = ToggleDeviceParameter(jsonDevice, i);
        }
    }

    private static string ToggleDeviceParameter(string jsonDevice, int i)
    {
        dynamic json = JsonConvert.DeserializeObject(jsonDevice);

        json.Params.Command = i % 2 == 0
            ? "p 3 255 p 4 255 p 14 255"
            : "p 3 0 p 4 0 p 14 0";

        jsonDevice = JsonConvert.SerializeObject(json);
        return jsonDevice;
    }
}
