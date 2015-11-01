using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PiCandy.Server.OPC
{
    public class OpcCommandListenerTests
    {
        [Fact]
        public void SampleMessages_ReceivedCorrectly()
        {
            var messages = new List<OpcMessage>();
            var endpoint = new IPEndPoint(IPAddress.Loopback, 8001);
            using (var server = new OpcCommandListener(endpoint))
            {
                server.MessageReceived += (o, a) =>
                {
                    messages.Add(a);
                };

                SendBytes(endpoint, new byte[]{
                    0x05,
                    0x07,

                    0x00,
                    0x02,

                    // payload
                    0x01,
                    0x02,


                    // Second message
                    0x03,
                    0x04,

                    0x00,
                    0x03,

                    // payload
                    0x01,
                    0x02,
                    0x03,
                });

                // Wait for pending async threads etc...
                System.Threading.Thread.Sleep(500);

                Assert.NotEmpty(messages);

                var message = messages[0];
                Assert.Equal(5, message.Channel);
                Assert.Equal(7, (int)message.Command);
                Assert.Equal(2, message.Length);
                Assert.Equal(2, message.Data.Length);
                Assert.Equal(new byte[] { 0x01, 0x02 }, message.Data);

                message = messages[1];
                Assert.Equal(3, message.Channel);
                Assert.Equal(4, (int)message.Command);
                Assert.Equal(3, message.Length);
                Assert.Equal(3, message.Data.Length);
                Assert.Equal(new byte[] { 0x01, 0x02, 0x03 }, message.Data);
            }
        }

        private void SendBytes(IPEndPoint endpoint, byte[] buffer)
        {
            using (var client = new System.Net.Sockets.TcpClient())
            {
                client.Connect(endpoint);
                
                var stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                client.Close();
            }
        }
    }
}
