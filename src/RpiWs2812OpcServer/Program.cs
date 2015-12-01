using PiCandy.Server;
using PiCandy.Server.OPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PiCandy.Server.Renderers;
using System.Collections.Concurrent;
using PiCandy.Logging;
using PiCandy.Rendering;

namespace RpiWs2812OpcServer
{
    class Program
    {
        const int defaultPort = 7890; // same as OpenPixelControl reference impl. uses
        static PiCandy.Renderer.RpiWs2812.RpiWs281xClient _renderer;
        static readonly ILog Log = new ConsoleLogger(); // { IsVerboseEnabled = true };
        static ConcurrentQueue<OpcMessage> Queue = new ConcurrentQueue<OpcMessage>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Console.WriteLine("Starting up...");
            int port;
            if (!(args.Length > 0 && int.TryParse(args[0], out port)))
                port = defaultPort;

            WriteEndpointsBanner(port);

            var cancel = new CancellationTokenSource();
            // This structure an attempt to co-erce messages onto origin thread for Ws2812 queue
            var worker = Task.Run(() => RunWorker(port, cancel.Token), cancel.Token);
            try
            {
                Console.WriteLine("Press RETURN to close server");
                Console.ReadLine();
                Console.WriteLine("Closing server");
            }
            catch (Exception err)
            {
                Console.WriteLine("Aborting server");
                Console.WriteLine(err);
            }
            finally
            {
                Console.WriteLine("Cancelling worker and waiting for 2 secs");
                cancel.Cancel();
                worker.Wait(TimeSpan.FromSeconds(2));
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating)
                Log.Error("Unhandled exception, aborting", e.ExceptionObject as Exception);
        }

        private static void RunWorker(int port, CancellationToken token)
        {
            using (_renderer = new PiCandy.Renderer.RpiWs2812.RpiWs281xClient(60))
            using (var listener = new SimpleSocketServer<OpcReader>(
                IPAddress.Any,
                port,
                CreateClient,
                Log
                ))
            {
                // RunSelfTest(_renderer);

                while (!token.IsCancellationRequested)
                {
                    OpcMessage message;
                    if (Queue.TryDequeue(out message))
                    {
                        Log.Verbose("Processing queued message...");
                        HandleMessageReceived(null, message);
                    }
                    else
                    {
                        Log.Verbose("Sleeping and try again");
                        Thread.Sleep(10);
                    }
                }

                Log.Warn("Cancel requested - aborting");
            }
        }

        private static void RunSelfTest(IPixelRenderer renderer)
        {
            Log.InfoFormat("Running self-test on {0}: {1} pixels", renderer, renderer.PixelCount);
            Log.InfoFormat("Red");
            for (int i = 0; i < renderer.PixelCount; i++)
            {
                renderer.SetPixels(n => n == i ? 0xff0000 : 0x000000);
                renderer.Show();
                Thread.Sleep(50);
            }

            Log.InfoFormat("Green");
            for (int i = 0; i < renderer.PixelCount; i++)
            {
                renderer.SetPixels(n => n == i ? 0x00ff00 : 0x000000);
                renderer.Show();
                Thread.Sleep(50);
            }

            Log.InfoFormat("Blue");
            for (int i = 0; i < renderer.PixelCount; i++)
            {
                renderer.SetPixels(n => n == i ? 0x0000ff : 0x000000);
                renderer.Show();
                Thread.Sleep(50);
            }
        }

        private static void CreateClient(System.Net.Sockets.TcpClient client)
        {
            var ep = client.Client.RemoteEndPoint;
            Log.InfoFormat("Client {0} connected", ep);
            var session = new OpcReader(client, Log);

            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await session.ReadMessageAsync().ConfigureAwait(false);
                    if (message == null)
                    {
                        Log.InfoFormat("Client {0} disconnected", ep);
                        return;
                    }

                    // We don't need this queuing. That wasn't the problem anyway!
                    Queue.Enqueue(message.Value);
                    //HandleMessageReceived(session, message);
                }
            });
        }

        private static void HandleMessageReceived(object sender, OpcMessage message)
        {
            Log.DebugFormat("Handling message received {0}", message);
            if (message.Command == OpcCommandType.SetPixels)
            {
                // render no more data than we can (pixel count)
                for (int i = 0; i < _renderer.PixelCount; i++)
                {
                    var r = message.Data[i * 3];
                    var g = message.Data[i * 3 + 1];
                    var b = message.Data[i * 3 + 2];

                    _renderer.SetPixelColor(i, r, g, b);
                }
                _renderer.Show();
            }
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
