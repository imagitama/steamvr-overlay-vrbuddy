using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace VRBuddy
{
    class Receiver
    {
        public Receiver(ref Settings settings, ref Render render)
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
                    var trackingData = TrackingData.FromJson(jsonData);

                    if (settings.Debug) {
                        Console.WriteLine("");

                        Console.WriteLine($"HMD Position: {trackingData.Hmd.Position.X}, {trackingData.Hmd.Position.Y}, {trackingData.Hmd.Position.Z}");
                        Console.WriteLine($"HMD Rotation: {trackingData.Hmd.Rotation.X}, {trackingData.Hmd.Rotation.Y}, {trackingData.Hmd.Rotation.Z}, {trackingData.Hmd.Rotation.W}");
                        Console.WriteLine($"Left Hand Position: {trackingData.LeftHand.Position.X}, {trackingData.LeftHand.Position.Y}, {trackingData.LeftHand.Position.Z}");
                        Console.WriteLine($"Left Hand Rotation: {trackingData.LeftHand.Rotation.X}, {trackingData.LeftHand.Rotation.Y}, {trackingData.LeftHand.Rotation.Z}, {trackingData.LeftHand.Rotation.W}");
                        Console.WriteLine($"Right Hand Position: {trackingData.RightHand.Position.X}, {trackingData.RightHand.Position.Y}, {trackingData.RightHand.Position.Z}");
                        Console.WriteLine($"Right Hand Rotation: {trackingData.RightHand.Rotation.X}, {trackingData.RightHand.Rotation.Y}, {trackingData.RightHand.Rotation.Z}, {trackingData.RightHand.Rotation.W}");
                    }

                    render.RenderTrackingData(trackingData, settings.Offset.Position, settings.Offset.Rotation, settings.LookAtPlayer);
                }
            }
        }
    }
}