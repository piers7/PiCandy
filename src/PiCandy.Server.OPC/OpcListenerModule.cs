using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiCandy.Server.OPC
{
    /// <summary>
    /// Configures the system to listen for Open Pixel Protocol messages
    /// on a particular port and raise commands accordingly
    /// </summary>
    public class OpcListenerModule : Module
    {
        /// <summary>
        /// Optional: specifies listener is only active on one IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Required: specify what port the listener listens on
        /// </summary>
        public int Port { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var ip = string.IsNullOrWhiteSpace(IpAddress)
                ? System.Net.IPAddress.Any
                : System.Net.IPAddress.Parse(IpAddress)
                ;
            var endpoint = new System.Net.IPEndPoint(ip, Port);

            // Obviously bad things will happen if multiple services
            // attempt to resolve the listener
            // but can't just be SingleInstance() as might have multiple
            // as long as *setup on different ports*
            builder.RegisterType<OpcCommandListener>()
                .WithParameter(TypedParameter.From(endpoint))
                .AsSelf()
                .AsImplementedInterfaces()
                ;
        }
    }
}
