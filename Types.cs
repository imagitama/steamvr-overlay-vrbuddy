using SharpDX;
using Newtonsoft.Json;

namespace VRBuddy
{
    public class Settings {
        public ulong SteamFriendId { get; set; }
        public string IpAddress { get; set; }
        public int InboundPort { get; set; }
        public int OutboundPort { get; set; }
        public float Scale { get; set; }
        public float HeadScale { get; set; }
        public float HandScale { get; set; }
        public Transform Offset { get; set; }
        public bool LookAtPlayer { get; set; }
        public bool SendData { get; set; }
        public bool ReceiveData { get; set; }
        // not exposed
        public bool Debug { get; set; }
        public bool VirtualPlayspace { get; set; }

        public string ToJson()
        {
            var jsonData = new
            {
                SteamFriendId = SteamFriendId,
                IpAddress = IpAddress,
                InboundPort = InboundPort,
                OutboundPort = OutboundPort,
                Scale = Scale,
                HeadScale = HeadScale,
                HandScale = HandScale,
                Offset = new
                {
                    Position = new SimpleVector3(Offset.Position.X, Offset.Position.Y, Offset.Position.Z),
                    Rotation = new SimpleQuaternion(Offset.Rotation.X, Offset.Rotation.Y, Offset.Rotation.Z, Offset.Rotation.W)
                },
                LookAtPlayer = LookAtPlayer,
                SendData = SendData,
                ReceiveData = ReceiveData,
                Debug = Debug,
                VirtualPlayspace = VirtualPlayspace,
            };

            return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
        }
    }

    public class TrackingData
    {
        public Transform Hmd { get; set; }
        public Transform LeftHand { get; set; }
        public Transform RightHand { get; set; }

        public string ToJson()
        {
            var jsonData = new
            {
                Hmd = new
                {
                    Position = new SimpleVector3(Hmd.Position.X, Hmd.Position.Y, Hmd.Position.Z),
                    Rotation = new SimpleQuaternion(Hmd.Rotation.X, Hmd.Rotation.Y, Hmd.Rotation.Z, Hmd.Rotation.W)
                },
                LeftHand = new
                {
                    Position = new SimpleVector3(LeftHand.Position.X, LeftHand.Position.Y, LeftHand.Position.Z),
                    Rotation = new SimpleQuaternion(LeftHand.Rotation.X, LeftHand.Rotation.Y, LeftHand.Rotation.Z, LeftHand.Rotation.W)
                },
                RightHand = new
                {
                    Position = new SimpleVector3(RightHand.Position.X, RightHand.Position.Y, RightHand.Position.Z),
                    Rotation = new SimpleQuaternion(RightHand.Rotation.X, RightHand.Rotation.Y, RightHand.Rotation.Z, RightHand.Rotation.W)
                }
            };

            return JsonConvert.SerializeObject(jsonData);
        }

        public static TrackingData FromJson(string json)
        {
            var jsonData = JsonConvert.DeserializeObject<JsonData>(json);

            return new TrackingData
            {
                Hmd = new Transform
                {
                    Position = new SimpleVector3
                    {
                        X = jsonData.Hmd.Position.X,
                        Y = jsonData.Hmd.Position.Y,
                        Z = jsonData.Hmd.Position.Z
                    },
                    Rotation = new SimpleQuaternion
                    {
                        X = jsonData.Hmd.Rotation.X,
                        Y = jsonData.Hmd.Rotation.Y,
                        Z = jsonData.Hmd.Rotation.Z,
                        W = jsonData.Hmd.Rotation.W
                    }
                },
                LeftHand = new Transform
                {
                    Position = new SimpleVector3
                    {
                        X = jsonData.LeftHand.Position.X,
                        Y = jsonData.LeftHand.Position.Y,
                        Z = jsonData.LeftHand.Position.Z
                    },
                    Rotation = new SimpleQuaternion
                    {
                        X = jsonData.LeftHand.Rotation.X,
                        Y = jsonData.LeftHand.Rotation.Y,
                        Z = jsonData.LeftHand.Rotation.Z,
                        W = jsonData.LeftHand.Rotation.W
                    }
                },
                RightHand = new Transform
                {
                    Position = new SimpleVector3
                    {
                        X = jsonData.RightHand.Position.X,
                        Y = jsonData.RightHand.Position.Y,
                        Z = jsonData.RightHand.Position.Z
                    },
                    Rotation = new SimpleQuaternion
                    {
                        X = jsonData.RightHand.Rotation.X,
                        Y = jsonData.RightHand.Rotation.Y,
                        Z = jsonData.RightHand.Rotation.Z,
                        W = jsonData.RightHand.Rotation.W
                    }
                }
            };
        }

        private class JsonData
        {
            public TransformJson Hmd { get; set; }
            public TransformJson LeftHand { get; set; }
            public TransformJson RightHand { get; set; }
        }

        private class TransformJson
        {
            public SimpleVector3 Position { get; set; }
            public SimpleQuaternion Rotation { get; set; }
        }
    }

    public class Transform
    {
        public SimpleVector3 Position { get; set; }
        public SimpleQuaternion Rotation { get; set; }
    }

    public class SimpleVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public SimpleVector3(float X = 0, float Y = 0, float Z = 0) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }

    public class SimpleQuaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public SimpleQuaternion(float X = 0, float Y = 0, float Z = 0, float W = 0) {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.W = W;
        }

        public SharpDX.Quaternion ToSharpDXQuaternion()
        {
            return new SharpDX.Quaternion(X, Y, Z, W);
        }
    }
}