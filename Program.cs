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
using SharpDX;

namespace VRBuddy
{
    class Program
    {
        const string filePath = "settings.json";
        static Settings settings;
        static float moveAmount = 0.05f;
        static float rotateAmount = 0.05f;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to VR Buddy!");
        
            settings = new Settings() {
                InboundPort = 5000,
                OutboundPort = 5000,
                Scale = 0.5f,
                HeadScale = 1f,
                HandScale = 1f,
                SendData = true,
                ReceiveData = true,
                Offset = new Transform() {
                    Position = new SimpleVector3(1f, 0, -1f),
                    Rotation = new SimpleQuaternion(0, 0, 0, 0),
                },
                LookAtPlayer = false,
                Debug = false
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

                // TODO
                // settings.LookAtPlayer = Input.GetInputBool("Make overlays look directly at you", settings.LookAtPlayer, false);

                Console.WriteLine("New settings:");
                OutputSettings(settings);
                Console.WriteLine("Press enter to save and continue (press any other key to cancel)");

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key != ConsoleKey.Enter) {
                    Console.WriteLine("");
                    Console.WriteLine("You did not press enter. Quitting...");
                    return;
                }
                
                SaveSettingsToFile();

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

            Console.WriteLine("");
            Console.WriteLine("  Use numpad to adjust offsets:");
            Console.WriteLine("  7     8     9         7: Backward      8: Up             9: Forward");
            Console.WriteLine("  4     5     6         4: Left          5: Reset          6: Right");
            Console.WriteLine("  1     2     3         1: Rotate Down   2: Down           3: Rotate Up");
            Console.WriteLine("  0           .         0: (not used)                      .: Rotate Right");
            Console.WriteLine("  -     +     *         -: Rotate Left   +: Rotate Right   *: Rotate Down");
            Console.WriteLine("");
            Console.WriteLine("  Press Spacebar to toggle debugging8");
            Console.WriteLine("");

            // VirtualPlayspace virtualPlayspace = new VirtualPlayspace();
            // var render = settings.VirtualPlayspace ? new RenderNonVR(settings, virtualPlayspace) : new Render(settings);

            var render = new Render(ref settings);

            var senderTask = settings.SendData ? Task.Run(() => StartSender(ref settings)) : Task.CompletedTask;
            var receiverTask = settings.ReceiveData ? Task.Run(() => StartReceiver(ref settings, ref render)) : Task.CompletedTask;
            var inputTask = HandleUserInput();

            if (settings.VirtualPlayspace) {
                // must be run on main thread
                // virtualPlayspace.Start();
            } else {
                await Task.WhenAll(senderTask, receiverTask, inputTask);
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
            Console.WriteLine($"  Render Offset Position: X={settings.Offset.Position.X}, Y={settings.Offset.Position.Y}, Z={settings.Offset.Position.Z}");
            Console.WriteLine($"  Render Offset Rotation: X={settings.Offset.Rotation.X}, Y={settings.Offset.Rotation.Y}, Z={settings.Offset.Rotation.Z}");
            Console.WriteLine($"  Sending: {(settings.SendData ? "Yes" : "No")}");
            Console.WriteLine($"  Receiving: {(settings.ReceiveData ? "Yes" : "No")}");
            Console.WriteLine($"  Look At You: {(settings.LookAtPlayer ? "Yes" : "No")}");
        }

        static void StartSender(ref Settings settings)
        {
            Console.WriteLine("Starting to send data...");
            var sender = new Sender(ref settings);
        }

        static void StartReceiver(ref Settings settings, ref Render render)
        {
            Console.WriteLine("Starting to receive data...");
            var receiver = new Receiver(ref settings, ref render);
        }

        static async Task HandleUserInput()
        {
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
                UpdateSettingsWithKey(key);
                await Task.Delay(100);
            } while (key != ConsoleKey.Escape);
        }

        static void UpdateSettingsWithKey(ConsoleKey key)
        {
            if (settings.Debug) {
                Console.Write($" {key.ToString()}");
            }

            switch (key)
            {
                case ConsoleKey.NumPad5: ResetOffset(); break;

                case ConsoleKey.NumPad8: settings.Offset.Position.Y += moveAmount; break;
                case ConsoleKey.NumPad2: settings.Offset.Position.Y -= moveAmount; break;
                case ConsoleKey.NumPad4: settings.Offset.Position.X -= moveAmount; break;
                case ConsoleKey.NumPad6: settings.Offset.Position.X += moveAmount; break;
                case ConsoleKey.NumPad7: settings.Offset.Position.Z -= moveAmount; break;
                case ConsoleKey.NumPad9: settings.Offset.Position.Z += moveAmount; break;

                case ConsoleKey.NumPad1: settings.Offset.Rotation.X -= rotateAmount; break;
                case ConsoleKey.NumPad3: settings.Offset.Rotation.X += rotateAmount; break;
                case ConsoleKey.Subtract: settings.Offset.Rotation.Y -= rotateAmount; break;
                case ConsoleKey.Add: settings.Offset.Rotation.Y += rotateAmount; break;
                case ConsoleKey.Multiply: settings.Offset.Rotation.Z -= rotateAmount; break;
                case ConsoleKey.Decimal: settings.Offset.Rotation.Z += rotateAmount; break;

                case ConsoleKey.Spacebar: settings.Debug = !settings.Debug; break;
            }

            SaveSettingsToFile();
        }

        static void SaveSettingsToFile()
        {
            string newJson = settings.ToJson();
            File.WriteAllText(filePath, newJson);
        }

        static void ResetOffset()
        {
            settings.Offset = new Transform() {
                Position = new SimpleVector3(1f, 0, 0),
                Rotation = new SimpleQuaternion(0, 0, 0, 0),
            };
        }
    }
}