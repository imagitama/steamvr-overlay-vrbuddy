using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Valve.VR;

namespace VRBuddy
{
    class Sender
    {
        public Sender(ref Settings settings)
        {
            Console.WriteLine($"Starting sender to {settings.IpAddress}:{settings.OutboundPort}");

            var tracking = new Tracking(settings);

            using (UdpClient udpClient = new UdpClient())
            {
                while (true)
                {
                    var trackingData = tracking.GetTrackingData();
                    var jsonData = trackingData.ToJson();
                    var bytes = Encoding.UTF8.GetBytes(jsonData);

                    if (settings.Debug) {
                        Console.WriteLine(jsonData);
                    }

                    udpClient.Send(bytes, bytes.Length, settings.IpAddress, settings.OutboundPort);

                    Thread.Sleep(20);
                }
            }
        }
    }
}