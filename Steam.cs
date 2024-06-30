using System;
using System.Text;
using Steamworks;

namespace VRBuddy
{
    class Steam
    {
        public static void OutputMyself()
        {
            CSteamID mySteamID = SteamUser.GetSteamID();
            string myName = SteamFriends.GetPersonaName();

            Console.WriteLine("Your Steam info: " + myName + " (" + mySteamID + ")");
        }

        public static ulong PickFriendId() {
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            List<(string FriendName, CSteamID FriendSteamID)> friends = new List<(string, CSteamID)>();

            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string friendName = SteamFriends.GetFriendPersonaName(friendSteamID);
                friends.Add((friendName, friendSteamID));
            }

            var sortedFriends = friends.OrderBy(f => f.FriendName).ToList();

            for (int i = 0; i < sortedFriends.Count; i++)
            {
                var friend = sortedFriends[i];
                Console.WriteLine((i + 1) + ": " + friend.FriendName + " (" + friend.FriendSteamID + ")");
            }

            Console.WriteLine("Enter the number of the friend you want to connect to:");

            string input = Console.ReadLine().Trim().ToLower();

            if (int.TryParse(input, out int friendNumber))
            {
                int friendIndex = friendNumber - 1;

                if (friendIndex >= 0 && friendIndex < sortedFriends.Count)
                {
                    CSteamID selectedFriendSteamID = sortedFriends[friendIndex].FriendSteamID;
                    Console.WriteLine("Connecting to: " + sortedFriends[friendIndex].FriendName);
                    return (ulong)selectedFriendSteamID;
                }
                else
                {
                    Console.WriteLine("Invalid friend number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number.");
            }

            return 0;
        }

        public static void StartListening(ref Settings settings, ref Render render)
        {
            var allowedRemoteId = new CSteamID(settings.SteamFriendId);

            if (settings.Debug) {
                Console.WriteLine("Steam.listen");
            }
            
            while (true)
            {
                uint packetSize;

                while (SteamNetworking.IsP2PPacketAvailable(out packetSize))
                {
                    byte[] buffer = new byte[packetSize];
                    uint bytesRead;
                    CSteamID remoteId;

                    if (SteamNetworking.ReadP2PPacket(buffer, packetSize, out bytesRead, out remoteId))
                    {
                        if (remoteId != allowedRemoteId) {
                            Console.WriteLine("Error - wrong remote ID!");
                            break;
                        }

                        var jsonData = System.Text.Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
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

                Thread.Sleep(100);
            }
        }

        public static void StartSending(ref Settings settings)
        {
            var tracking = new Tracking(settings);
            var recipient = new CSteamID(settings.SteamFriendId);

            while (true)
            {
                var trackingData = tracking.GetTrackingData();
                var jsonData = trackingData.ToJson();
                var bytes = Encoding.UTF8.GetBytes(jsonData);

                if (settings.Debug) {
                    Console.WriteLine(jsonData);
                }

                SteamNetworking.SendP2PPacket(recipient, bytes, (uint)bytes.Length, EP2PSend.k_EP2PSendReliable);

                Thread.Sleep(20);
            }
        }
    }
}