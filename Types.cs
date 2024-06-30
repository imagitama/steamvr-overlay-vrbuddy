namespace VRBuddy
{
    public class Settings {
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
    }

    public class TrackingData
    {
        public Transform Hmd { get; set; }
        public Transform LeftHand { get; set; }
        public Transform RightHand { get; set; }
    }

    public class Transform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public class Vector3
    {
        public Vector3(float x = 0, float y = 0, float z = 0) {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    public class Quaternion
    {
        public Quaternion(float x = 0, float y = 0, float z = 0, float w = 0) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }
        public static Quaternion FromEuler(float pitch, float yaw, float roll) {
            float c1 = MathF.Cos(yaw / 2);
            float c2 = MathF.Cos(pitch / 2);
            float c3 = MathF.Cos(roll / 2);
            float s1 = MathF.Sin(yaw / 2);
            float s2 = MathF.Sin(pitch / 2);
            float s3 = MathF.Sin(roll / 2);

            return new Quaternion(
                c1 * c2 * s3 - s1 * s2 * c3, // x
                c1 * s2 * c3 + s1 * c2 * s3, // y
                s1 * c2 * c3 - c1 * s2 * s3, // z
                c1 * c2 * c3 + s1 * s2 * s3  // w
            );
        }
    }
}