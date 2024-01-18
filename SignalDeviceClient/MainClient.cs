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

        var jsonDeviceStatus = JsonConvert.SerializeObject(device);

        await connection.InvokeAsync("GetDeviceStatusInitial", jsonDeviceStatus, device.NameId);

        for (var i = 0; i < 60; i++)
        {
            jsonDeviceStatus = ToggleDeviceParameter(jsonDeviceStatus, i);

            WriteDeviceParamsToConsole(i, jsonDeviceStatus);
            await connection.InvokeAsync("DeviceStatusSend", jsonDeviceStatus, device.NameId);
            await connection.InvokeAsync("SendCommand", jsonDeviceStatus);
            await Task.Delay(1000);
        }
    }

    private static string ToggleDeviceParameter(string jsonDevice, int i)
    {
        dynamic json = JsonConvert.DeserializeObject(jsonDevice)!;

        json.Params.Brightness = i%2 == 0 
            ? 100 
            : 0;

        return JsonConvert.SerializeObject(json);
    }

    private static void WriteDeviceParamsToConsole(int i, string jsonDeviceStatus)
    {
        Console.WriteLine($"Count: {i:D4}");
        Console.WriteLine($"{jsonDeviceStatus}");
    }
}
