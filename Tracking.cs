using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Valve.VR;
using SharpDX;

namespace VRBuddy
{
    class Tracking
    {
        uint hmdIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
        uint leftHandIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
        uint rightHandIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

        public Tracking(Settings settings)
        {
            if (settings.Debug) {
                Console.WriteLine("Tracking.init");
            }
        }

        public TrackingData GetTrackingData()
        {
            TrackingData trackingData = new TrackingData
            {
                Hmd = new Transform(),
                LeftHand = new Transform(),
                RightHand = new Transform()
            };

            var system = Valve.VR.OpenVR.System;

            TrackedDevicePose_t[] poses = new TrackedDevicePose_t[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];
            system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

            TrackingData data = new TrackingData();

            if (poses[0].bPoseIsValid)
            {
                trackingData.Hmd = GetTransform(poses[0]);
            }

            if (poses[1].bPoseIsValid)
            {
                trackingData.LeftHand = GetTransform(poses[1]);
            }

            if (poses[2].bPoseIsValid)
            {
                trackingData.RightHand = GetTransform(poses[2]);
            }

            return trackingData;
            }

        private Transform GetTransform(TrackedDevicePose_t pose)
        {
            var matrix = pose.mDeviceToAbsoluteTracking;

            Transform transform = new Transform
            {
                Position = new SimpleVector3
                {
                    X = matrix.m3,
                    Y = matrix.m7,
                    Z = matrix.m11
                },
                Rotation = new SimpleQuaternion
                {
                    X = matrix.m0,
                    Y = matrix.m1,
                    Z = matrix.m2,
                    W = matrix.m9
                }
            };

            return transform;
        }
    }
}