using Hardware;
using Domotica.Server.Execute;
using Microsoft.AspNetCore.SignalR;

namespace Domotica.Server.Hubs
{
    public sealed class Device : Hub
    {
        // Container for device status implemented on the device html page.
        // Every device knows what it is and how to deal with related data!
        public static void SendCommand(string value)
        {
            Command.Execute(value);
        }

        public async Task DeviceStatusSend(string device, string group)
        {
            await Devices.AddOrUpdate(@group, device);
            await Clients.OthersInGroup(group).SendAsync("deviceStatusReceived", device);
        }

        public async Task GetDeviceStatusInitial(string device, string group)
        {
            var (ok, storedDevice) = await Devices.Read(group);

            // check if device is stored
            switch (ok)
            {
                // no stored device found: add a new one
                case false when !string.IsNullOrEmpty(storedDevice):
                    await Devices.AddOrUpdate(@group, device);          
                    break;

                default:
                    device = string.IsNullOrEmpty(storedDevice) 
                        ? device                                    // no stored device found: take the html page template
                        : storedDevice;                             // take stored device    
                    break;
            }

            await JoinGroup(group);
            await Clients.Caller.SendAsync("deviceStatusInitial", device);
        }
        
        public async Task SetDeviceStatusFinal(string group)
        {
            await LeaveGroup(group);
        }

        private async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        private async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
