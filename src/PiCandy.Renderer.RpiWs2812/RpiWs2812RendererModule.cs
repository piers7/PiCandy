using Autofac;
using PiCandy.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Renderer.RpiWs2812
{
    /// <summary>
    /// Module to register a channel for use with the libws2811 library on Raspberry Pi
    /// </summary>
    public class RpiWs2812RendererModule : Module
    {
        readonly int _channel;
        readonly RpiWs281xSetupInfo _settings;

        public RpiWs2812RendererModule(int channel, int pixels)
        {
            _channel = channel;
            _settings = new RpiWs281xSetupInfo(pixels);
        }

        /// <summary>
        /// Defines the channel the renderer is exposed to clients as
        /// </summary>
        public int Channel { get { return _channel; } }

        /// <summary>
        /// Defines the number of pixels the renderer exposes
        /// </summary>
        public int Pixels { get { return _settings.LedCount; } }

        public string Map { get; set; }

        #region Expose other from RpiWs281xSetupInfo (easier to config this way
        public uint Frequency
        {
            get { return _settings.Frequency; }
            set { _settings.Frequency = value; }
        }
        public int Dma
        {
            get { return _settings.Dma; }
            set { _settings.Dma = value; }
        }
        public int GpioPin
        {
            get { return _settings.GpioPin; }
            set { _settings.GpioPin = value; }
        }
        public bool Invert
        {
            get { return _settings.Invert; }
            set { _settings.Invert = value; }
        }
        public byte Brightness
        {
            get { return _settings.Brightness; }
            set { _settings.Brightness = value; }
        }
        #endregion

        protected override void Load(ContainerBuilder builder)
        {
            // This will only work on RPi platform with libws2811 available
            if(Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Console.WriteLine("WARNING: {0} unavailable, as only works on Raspberry Pi", GetType().Name);
                return;
            }

            builder
                .RegisterType<RpiWs281xClient>()
                .WithParameter(TypedParameter.From(_settings))
                .As<IPixelRenderer>()
                .WithMetadata<ChannelMetadata>(m =>
                {
                    m.For(am => am.Channel, Channel);
                    m.For(am => am.Map, Map);
                }
                )
                ;
        }
    }
}
