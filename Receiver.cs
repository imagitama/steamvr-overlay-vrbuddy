using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace VRBuddy
{
    class Receiver
    {
        public Receiver(Settings settings, ref Render render)
        {
            if (settings.Debug) {
                Console.WriteLine("Receiver.init");
            }

            Console.WriteLine($"Receiving on {settings.InboundPort}");

            using (UdpClient udpClient = new UdpClient(settings.InboundPort))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, settings.InboundPort);

                while (true)
                {
                    var receivedBytes = udpClient.Receive(ref remoteEndPoint);
                    var jsonData = Encoding.UTF8.GetString(receivedBytes);
                    var trackingData = JsonSerializer.Deserialize<TrackingData>(jsonData);

                    if (settings.Debug) {
                        Console.WriteLine("");

                        Console.WriteLine($"HMD Position: {trackingData.Hmd.Position.x}, {trackingData.Hmd.Position.y}, {trackingData.Hmd.Position.z}");
                        Console.WriteLine($"HMD Rotation: {trackingData.Hmd.Rotation.x}, {trackingData.Hmd.Rotation.y}, {trackingData.Hmd.Rotation.z}, {trackingData.Hmd.Rotation.w}");
                        Console.WriteLine($"Left Hand Position: {trackingData.LeftHand.Position.x}, {trackingData.LeftHand.Position.y}, {trackingData.LeftHand.Position.z}");
                        Console.WriteLine($"Left Hand Rotation: {trackingData.LeftHand.Rotation.x}, {trackingData.LeftHand.Rotation.y}, {trackingData.LeftHand.Rotation.z}, {trackingData.LeftHand.Rotation.w}");
                        Console.WriteLine($"Right Hand Position: {trackingData.RightHand.Position.x}, {trackingData.RightHand.Position.y}, {trackingData.RightHand.Position.z}");
                        Console.WriteLine($"Right Hand Rotation: {trackingData.RightHand.Rotation.x}, {trackingData.RightHand.Rotation.y}, {trackingData.RightHand.Rotation.z}, {trackingData.RightHand.Rotation.w}");
                    }

                    render.RenderTrackingData(trackingData, settings.Offset.Position, settings.Offset.Rotation);
                }
            }
        }
    }
}