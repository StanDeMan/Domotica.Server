using SysNet = System.Net; 

namespace Hardware
{
    public class Platform
    {
        public enum EnmOperatingSystem
        {
            Unknown,
            Windows,
            Linux,
            Mac
        }

        private const string GpioFile = "/dev/pigpio";

        public static EnmOperatingSystem OperatingSystem { get; set; }
        public static string? DevicePath { get; set; }
        public static string? Dns { get; set; }

        static Platform()
        {
            // If not running on pi set environment for windows platform
            OperatingSystem = Environment.OSVersion.Platform != PlatformID.Win32NT 
                ? RunOnLinux() 
                : RunOnWindows();
        }

        /// <summary>
        /// Rebuild the path to windows convention:
        /// This is for debugging and simulating purpose 
        /// of the gpio if run under windows
        /// </summary>
        /// <param name="path">Linux path convention</param>
        private static void SetPath(string? path)
        {
            var currentPath = Path.GetFullPath(@"..\..\");  
            path = path?.TrimStart('/').Replace('/', '\\');
            DevicePath = Path.Combine(currentPath, path ?? "");
        }

        private static EnmOperatingSystem RunOnWindows()
        {
            DevicePath = Directory.GetCurrentDirectory() + GpioFile;
            SetPath(DevicePath);

            return EnmOperatingSystem.Windows;
        }

        private static EnmOperatingSystem RunOnLinux()
        {
            DevicePath = GpioFile;
            Dns = $"{SysNet.Dns.GetHostName()}.local";

            return EnmOperatingSystem.Linux;
        }
    }
}
