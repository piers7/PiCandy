using PiCandy.Server.OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace OpcListenerConsole
{
    class Program
    {
        const int defaultPort = 7890; // same as OpenPixelControl reference impl. uses

        static void Main(string[] args)
        {
            int port;
            if (!(args.Length > 0 && int.TryParse(args[0], out port)))
                port = defaultPort;

            using (var listener = new SimpleSocketServer<OpcReader>(
                IPAddress.Any, 
                port, 
                CreateClient
                ))
            {
                WriteEndpointsBanner(port);

                Console.WriteLine("Press RETURN to close server");
                Console.ReadLine();
            }
        }

        private static void CreateClient(System.Net.Sockets.TcpClient client)
        {
            Console.WriteLine("Handling client connect from {0}", client.Client.RemoteEndPoint);
            var reader = new OpcReader(client);
        }

        private static void HandleMessageReceived(OpcMessage message)
        {
            Console.WriteLine(message.ToString(12));
        }

        private static void WriteEndpointsBanner(int port)
        {
            Console.WriteLine("Listening for clients on the following endpoints:");
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces()
                    .Where(i => i.OperationalStatus == OperationalStatus.Up)
                    .OrderBy(i => i.Speed)
                )
            {
                // How to filter out all the junk I have from VPNs, virtual VM adapters etc...?
                foreach (var address in iface.GetIPProperties().UnicastAddresses
                    .Where(a =>
                        !a.IsTransient
                        && a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                )
                {
                    Console.WriteLine("\t{0}:{1} ({2})", address.Address, port, iface.Name);
                }
            }
        }
    }
}
