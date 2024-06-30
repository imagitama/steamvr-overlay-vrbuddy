using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Valve.VR;
using Newtonsoft.Json;
using System.Runtime.Loader;

namespace VRBuddy
{
    class Program
    {
        const string filePath = "settings.json";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to VR Buddy!");
        
            Settings settings = new Settings() {
                InboundPort = 5000,
                OutboundPort = 5000,
                Scale = 0.5f,
                HeadScale = 1f,
                HandScale = 1f,
                Offset = new Transform() {
                    Position = new Vector3(0, 0, 0),
                    Rotation = new Quaternion(0, 0, 0, 0),
                },
                Debug = true
            };

            bool askForSettings = true;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                settings = JsonConvert.DeserializeObject<Settings>(json);

                Console.WriteLine("");
                Console.WriteLine("Current settings:");
                OutputSettings(settings);

                Console.WriteLine("");
                Console.WriteLine("Press Enter to use these values or press any other key to change it");

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                askForSettings = (keyInfo.Key != ConsoleKey.Enter);
            }

            if (askForSettings) {
                Console.WriteLine("");

                settings.IpAddress = Input.GetInput("Enter your buddies IP address", settings.IpAddress, "127.0.0.1");
                settings.InboundPort = Input.GetInputInt("Enter the port number to listen", settings.InboundPort, 5000);
                settings.OutboundPort = Input.GetInputInt("Enter the port number to send", settings.OutboundPort, 5000);
                settings.SendData = Input.GetInputBool("Do you want to transmit data?", settings.SendData, true);
                settings.ReceiveData = Input.GetInputBool("Do you want to receive data?", settings.ReceiveData, true);
                settings.Scale = Input.GetInputFloat("Enter the initial size (meters) of the head and hands (0.5 = 50cm)", settings.Scale, 0.5f);
                settings.HeadScale = Input.GetInputFloat("Enter percent to scale head (0.5 = 50%)", settings.Scale, 1f);
                settings.HandScale = Input.GetInputFloat("Enter percent to scale hands (0.5 = 50%)", settings.Scale, 1f);

                if (settings.Offset == null)
                {
                    settings.Offset = new Transform();
                }

                settings.Offset.Position.x = Input.GetInputFloat("Enter the render offset position X (where 1 is 1m to right, -1 is 1m to left)", settings.Offset.Position.x, 1f);
                settings.Offset.Position.y = Input.GetInputFloat("Enter the render offset position Y (where 1 is 1m forwards, -1 is 1m backwards)", settings.Offset.Position.y, 0f);
                settings.Offset.Position.z = Input.GetInputFloat("Enter the render offset position Z (where 1 is 1m in the air, -1 is 1m in the ground)", settings.Offset.Position.z, 0f);
                settings.Offset.Rotation.x = Input.GetInputFloat("Enter the render offset rotation X (where 1 is facing left/right)", settings.Offset.Rotation.x, 0f);
                settings.Offset.Rotation.y = Input.GetInputFloat("Enter the render offset rotation Y (where 1 is tilt forwards/backwards)", settings.Offset.Rotation.y, 0f);
                settings.Offset.Rotation.z = Input.GetInputFloat("Enter the render offset rotation Z (where 1 is ear to ground)", settings.Offset.Rotation.z, 0f);

                settings.LookAtPlayer = Input.GetInputBool("Make overlays look directly at you", settings.LookAtPlayer, true);

                Console.WriteLine("New settings:");
                OutputSettings(settings);
                Console.WriteLine("Press enter to save and continue (press any other key to cancel)");

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key != ConsoleKey.Enter) {
                    Console.WriteLine("");
                    Console.WriteLine("You did not press enter. Quitting...");
                    return;
                }
                
                string newJson = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, newJson);

                Console.WriteLine("New settings saved");
            }

            Console.WriteLine("Starting...");

            if (!settings.VirtualPlayspace) {
                EVRInitError error = EVRInitError.None;
                var vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

                if (error != EVRInitError.None)
                {
                    Console.WriteLine($"OpenVR Initialization Error: {error}");
                    return;
                }
            }

            // VirtualPlayspace virtualPlayspace = new VirtualPlayspace();
            // var render = settings.VirtualPlayspace ? new RenderNonVR(settings, virtualPlayspace) : new Render(settings);

            var render = new Render(settings);

            var senderTask = settings.SendData ? Task.Run(() => StartSender(settings)) : Task.CompletedTask;
            var receiverTask = settings.ReceiveData ? Task.Run(() => StartReceiver(settings, ref render)) : Task.CompletedTask;

            if (settings.VirtualPlayspace) {
                // must be run on main thread
                // virtualPlayspace.Start();
            } else {
                await Task.WhenAll(senderTask, receiverTask);
            }

            OpenVR.Shutdown();
        }

        static void OutputSettings(Settings settings)
        {
            Console.WriteLine($"  IP Address: {settings.IpAddress}");
            Console.WriteLine($"  In Port: {settings.InboundPort}");
            Console.WriteLine($"  Out Port: {settings.OutboundPort}");
            Console.WriteLine($"  Global Scale: {settings.Scale}m");
            Console.WriteLine($"  Head Scale: {settings.HeadScale}x");
            Console.WriteLine($"  Hand Scale: {settings.HandScale}x");
            Console.WriteLine($"  Render Offset Position: X={settings.Offset.Position.x}, Y={settings.Offset.Position.y}, Z={settings.Offset.Position.z}");
            Console.WriteLine($"  Render Offset Rotation: X={settings.Offset.Rotation.x}, Y={settings.Offset.Rotation.y}, Z={settings.Offset.Rotation.z}");
            Console.WriteLine($"  Sending: {(settings.SendData ? "Yes" : "No")}");
            Console.WriteLine($"  Receiving: {(settings.ReceiveData ? "Yes" : "No")}");
            Console.WriteLine($"  Look At You: {(settings.LookAtPlayer ? "Yes" : "No")}");
        }

        static void StartSender(Settings settings)
        {
            Console.WriteLine("Starting to send data...");
            var sender = new Sender(settings);
        }

        static void StartReceiver(Settings settings, ref Render render)
        {
            Console.WriteLine("Starting to receive data...");
            var receiver = new Receiver(settings, ref render);
        }
    }
}