using DataBase;
using Newtonsoft.Json;
using Serilog;

namespace Hardware
{
    /// <summary>
    /// Device state handling
    /// </summary>
    public static class Devices
    {
        // Container for device status implemented on the device html page.
        // Every device knows what it is and how to deal with related data!
        private static readonly Dictionary<string, string> DeviceList = new();
        private static readonly FileStore Store = new();

        public static async Task<bool> AddOrUpdate(string key, string device)
        {
            try
            {
                // update existing
                if(DeviceList.TryGetValue(key, out _))
                {
                    DeviceList[key] = device;
                    await Store.UpdateAsync(device);

                    return true;
                }
                
                // add new
                DeviceList.Add(key, device);
                await Store.InsertAsync(device);

                return true;

            }
            catch (Exception e)
            {
                Log.Error($"Devices.AddOrUpdate: {e}");

                return false;
            }
        }

        public static bool Delete(string key)
        {
            return DeviceList.Remove(key);
        }

        public static async Task<(bool, string)> Read(string key)
        {
            try
            {
                // check if present in list
                if(DeviceList.TryGetValue(key, out var value))
                    return (true, value);
                
                // check if present in database
                var device = await Store.ReadByClassIdAsync(key);

                return ((bool, string))(device?.ClassId == key 
                    ? (true, JsonConvert.SerializeObject(device)) 
                    : (false, string.Empty));
            }
            catch (Exception e)
            {
                Log.Error($"Devices.Read: {e}");

                return (false, string.Empty);
            }
        }

        public static int Count()
        {
            return DeviceList.Count;
        }
    }
}
