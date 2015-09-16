using Autofac;
using Autofac.Configuration;
using Autofac.Features.Metadata;
using OpenPixels.Server;
using OpenPixels.Server.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OpenPixels.ServerHost
{
    class Program
    {
        private static ILog _log;

        static int Main(string[] args)
        {
            var configBase = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var log4netConfig = Path.Combine(configBase, "log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(log4netConfig));
            try
            {
                _log = GetLogger(typeof(Program));
                using(var container = CreateContainer(GetLogger))
                    return Run(container);
            }
            finally
            {
                log4net.LogManager.Shutdown();
            }
        }

        private static ILog GetLogger(Type type)
        {
            var raw = log4net.LogManager.GetLogger(type);
            var wrapped = new Log4NetWrapper(raw);
            return wrapped;
        }

        private static IContainer CreateContainer<T>(Func<Type,T> logFactory)
        {
            var builder = new ContainerBuilder();

            // Add core services
            builder
                .RegisterType<OpenPixelsServer>()
                .SingleInstance()
                ;

            // map autofac's type to our internal meta type (attempting to cut off autofac reference leakage)
            builder
                .RegisterAdapter<Meta<Lazy<IPixelRenderer>>, Lazy<IPixelRenderer, ChannelInfo>>(
                    meta => new Lazy<IPixelRenderer, ChannelInfo>(meta.Value, new ChannelInfo(meta.Metadata))
                );

            builder.RegisterModule(new LogInjectionModule<T>(logFactory) { InjectProperties = true });

            // Add XML overrides
            // NB: file name for this case sensitive under mono/linux
            builder.RegisterModule(new XmlFileReader("components.config"));

            var container = builder.Build();
            return container;
        }

        private static int Run(IContainer container)
        {
            var serverType = typeof(OpenPixelsServer);
            Console.WriteLine("{0} v{1}", serverType.Name, serverType.Assembly.GetName().Version);
            using (var server = container.Resolve<OpenPixelsServer>())
            {
                Console.WriteLine("Using the following command sources:");
                foreach (var item in server.Sources)
                    Console.WriteLine("\t{0}", item);

                Console.WriteLine("Using the following channels:");
                foreach (var item in server.Channels.OrderBy(c => c.Key))
                    Console.WriteLine("\t{0,2}: {1}", item.Key, string.Join(", ", item.Select(c => c.GetType().Name)));

                // I don't know about you, but between the VMs and the VPNs
                // I seem to have a *heap* many IP addresses
                Console.WriteLine("Your IP4 addresses are:");
                foreach (var iface in NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.OperationalStatus == OperationalStatus.Up)
                    .OrderBy(i => i.Name))
                {
                    foreach (var address in iface.GetIPProperties().UnicastAddresses
                        .Where(a => !a.IsTransient && a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        )
                        Console.WriteLine("\t{0} ({1})", address.Address, iface.Name);
                }

                foreach (var renderer in server.Channels.SelectMany(c => c))
                {
                    Console.WriteLine("Running self-test on {0}", renderer);
                    renderer.SetPixelColor(0, 0xFF, 0x00, 0x00);
                    renderer.SetPixelColor(1, 0x00, 0xFF, 0x00);
                    renderer.SetPixelColor(2, 0x00, 0x00, 0xFF);
                    renderer.SetPixelColor(3, 0xFF, 0x00, 0x00);
                    renderer.SetPixelColor(4, 0x00, 0xFF, 0x00);
                    renderer.SetPixelColor(5, 0x00, 0x00, 0xFF);
                }

                Console.WriteLine("Press ENTER to shutdown");
                Console.ReadLine();
            }
            return 0;
        }
    }
}
