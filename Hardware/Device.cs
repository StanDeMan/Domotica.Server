using System.Device.Spi;
using System.Drawing;
using Iot.Device.Apa102;

namespace Hardware
{
    public sealed class Device : IDisposable
    {
        private readonly SpiDevice? _spiDevice;

        public enum EnmState
        {
            Off = 0,
            On = 1,
            None = 3
        }

        public Apa102? Apa102;
        
        public bool IsRunning { get; set; }
        public int Quantity { get; set; }
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Device constructor
        /// ATTENTION: Must be without constructor parameters
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public Device()
        {
            try
            {
                _spiDevice = SpiDevice.Create(new SpiConnectionSettings(0, 0)
                {
                    ClockFrequency = 20_000_000,        // hardcoded first: set to 20 MHz
                    DataFlow = DataFlow.MsbFirst,
                    Mode = SpiMode.Mode0                // ensure data is ready at clock rising edge
                });

                if (_spiDevice == null) 
                    throw new NullReferenceException("Cannot start SPI.");

                IsRunning = true;
            }
            catch (Exception)
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// Dimmer used over Reflection
        /// </summary>
        /// <param name="parameter">Dynamic parameter for dimmer control</param>
        public void Dimmer(dynamic parameter)
        {
            // check if SPI is initialized
            if (!IsRunning) return;

            // preset alpha and colors
            double a = 0;   
            var red = 0;     
            var green = 0;   
            var blue = 0;
            var ledAmount = 0;

            try
            {
                // read Apa102 specific parameter:
                a = Convert.ToDouble(parameter.Color.A) ?? 1;           // alpha -> set to max. brightness
                red   = Convert.ToInt32(parameter.Color.R) ?? 0;        // red
                green = Convert.ToInt32(parameter.Color.G) ?? 0;        // green
                blue  = Convert.ToInt32(parameter.Color.B) ?? 0;        // blue

                ledAmount = Convert.ToInt32(parameter.LedAmount) ?? 1;  // set min. one led
            }
            catch (Exception)
            {
                // catch silently -> colors are present to 0
            }

            // check again and set LED Quantity
            Quantity = ledAmount <= 0            
                ? 1 
                : ledAmount;

            // instantiate Apa102 device
            if(_spiDevice != null) Apa102 = new Apa102(_spiDevice, Quantity);   
            if(Apa102 == null) return;

            // check if in bounds
            var alpha = a > 1
                ? byte.MaxValue                                 // max. 255
                : (int)(Math.Round(a * byte.MaxValue));         // calculate alpha for value < 1

            Color = Color.FromArgb(alpha, red, green, blue);    // set LED Color
            Dim(alpha);                                         // dim all LED diodes
            Flush();                                            // write to all LED devices
        }

        /// <summary>
        /// Switch: color is white only
        /// </summary>
        /// <param name="state">On/Off</param>
        public void Switch(EnmState state)
        {
            // check if SPI is initialized
            if (!IsRunning) return;

            switch (state)
            {
                case EnmState.Off:
                    Off();
                    break;

                case EnmState.On:
                    On();
                    break;

                case EnmState.None:
                default:
                    Off();
                    break;
            }
        }

        private void Flush()
        {
            Apa102?.Flush();
        }

        private void Dim(int brightness)
        {
            for (var i = 0; i < Apa102?.Pixels.Length; i++)
            {
                Apa102.Pixels[i] = Color.FromArgb(brightness, Color.R, Color.G, Color.B);
            }

            Apa102?.Flush();
        }

        private void Off(int brightness = 0)
        {
            for (var i = 0; i < Apa102?.Pixels.Length; i++)
            {
                Apa102.Pixels[i] = Color.FromArgb(brightness, 0, 0, 0);
            }

            Apa102?.Flush();
        }

        private void On(int brightness = 255)
        {
            for (var i = 0; i < Apa102?.Pixels.Length; i++)
            {
                Apa102.Pixels[i] = Color.FromArgb(brightness, 255, 255, 255);
            }

            Apa102?.Flush();
        }

        public void Dispose()
        {
            Apa102?.Dispose();
            _spiDevice?.Dispose();
        }
    }
}