using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Valve.VR;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System.Drawing;
using System.Drawing.Imaging;
using StbImageSharp;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace VRBuddy
{
    class Render
    {
        private static ulong headOverlayHandle;
        private static ulong leftHandOverlayHandle;
        private static ulong rightHandOverlayHandle;
        private static Texture_t textureHead;
        private static Texture_t textureLeftHand;
        private static Texture_t textureRightHand;
        private bool lookAtHead = true;

        public Render(Settings settings)
        {
            if (settings.VirtualPlayspace) {
                return;
            }

            if (settings.Debug) {
                Console.WriteLine("Render.init");
            }
            
            headOverlayHandle = CreateOverlay(settings, "vrbuddy.overlay.head", "Head Overlay", settings.HeadScale);
            leftHandOverlayHandle = CreateOverlay(settings, "vrbuddy.overlay.lefthand", "Left Hand Overlay", settings.HandScale);
            rightHandOverlayHandle = CreateOverlay(settings, "vrbuddy.overlay.righthand", "Right Hand Overlay", settings.HandScale);

            textureHead = CreateTextureFromPng("texture_head.png");
            textureLeftHand = CreateTextureFromPng("texture_lefthand.png");
            textureRightHand = CreateTextureFromPng("texture_righthand.png");

            OpenVR.Overlay.ShowOverlay(headOverlayHandle);
            OpenVR.Overlay.ShowOverlay(leftHandOverlayHandle);
            OpenVR.Overlay.ShowOverlay(rightHandOverlayHandle);
        }

        ulong CreateOverlay(Settings settings, string key, string name, float scale)
        {
            if (settings.Debug) {
                Console.WriteLine($"Render.CreateOverlay {key} {name}");
            }
            ulong overlayHandle = 0;
            OpenVR.Overlay.CreateOverlay(key, name, ref overlayHandle);
            OpenVR.Overlay.SetOverlayWidthInMeters(overlayHandle, settings.Scale * scale);
            OpenVR.Overlay.SetOverlayInputMethod(overlayHandle, VROverlayInputMethod.None);
            return overlayHandle;
        }

        public void RenderTrackingData(TrackingData trackingData, Vector3 positionOffset, Quaternion rotationOffset)
        {
            var headData = new Pose
            {
                Position = new SharpDX.Vector3(trackingData.Hmd.Position.x, trackingData.Hmd.Position.y, trackingData.Hmd.Position.z),
                Rotation = new SharpDX.Vector3(trackingData.Hmd.Rotation.x, trackingData.Hmd.Rotation.y, trackingData.Hmd.Rotation.z)
            };

            var leftHandData = new Pose
            {
                Position = new SharpDX.Vector3(trackingData.LeftHand.Position.x, trackingData.LeftHand.Position.y, trackingData.LeftHand.Position.z),
                Rotation = new SharpDX.Vector3(trackingData.LeftHand.Rotation.x, trackingData.LeftHand.Rotation.y, trackingData.LeftHand.Rotation.z)
            };

            var rightHandData = new Pose
            {
                Position = new SharpDX.Vector3(trackingData.RightHand.Position.x, trackingData.RightHand.Position.y, trackingData.RightHand.Position.z),
                Rotation = new SharpDX.Vector3(trackingData.RightHand.Rotation.x, trackingData.RightHand.Rotation.y, trackingData.RightHand.Rotation.z)
            };

            SetOverlayTransform(headOverlayHandle, headData, positionOffset, rotationOffset);
            SetOverlayTransform(leftHandOverlayHandle, leftHandData, positionOffset, rotationOffset);
            SetOverlayTransform(rightHandOverlayHandle, rightHandData, positionOffset, rotationOffset);

            OpenVR.Overlay.SetOverlayTexture(headOverlayHandle, ref textureHead);
            OpenVR.Overlay.SetOverlayTexture(leftHandOverlayHandle, ref textureLeftHand);
            OpenVR.Overlay.SetOverlayTexture(rightHandOverlayHandle, ref textureRightHand);
        }

        void SetOverlayTransform(ulong overlayHandle, Pose pose, Vector3 positionOffset, Quaternion rotationOffset)
        {
            var headPose = GetHeadPose();

            SharpDX.Vector3 direction = new SharpDX.Vector3(
                headPose.Position.X - pose.Position.X,
                headPose.Position.Y - pose.Position.Y,
                headPose.Position.Z - pose.Position.Z
            );

            direction.Normalize();

            float yaw = (float)Math.Atan2(direction.X, direction.Z);

            var transform = new HmdMatrix34_t();

            transform.m3 = pose.Position.X + positionOffset.x;
            transform.m7 = pose.Position.Y + positionOffset.y;
            transform.m11 = pose.Position.Z + positionOffset.z;

            if (lookAtHead) {
                transform.m0 = (float)Math.Cos(yaw);
                transform.m1 = 0;
                transform.m2 = (float)Math.Sin(yaw);
                transform.m4 = 0;
                transform.m5 = 1;
                transform.m6 = 0;
                transform.m8 = -(float)Math.Sin(yaw);
                transform.m9 = 0;
                transform.m10 = (float)Math.Cos(yaw);
            }

            SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);
        }

        public virtual void SetOverlayTransformAbsolute(ulong ulOverlayHandle, ETrackingUniverseOrigin eTrackingOrigin, ref HmdMatrix34_t pmatTrackingOriginToOverlayTransform) {
            OpenVR.Overlay.SetOverlayTransformAbsolute(ulOverlayHandle, eTrackingOrigin, ref pmatTrackingOriginToOverlayTransform);
        }

        static Pose GetHeadPose()
        {
            var headPose = new Pose();
            var system = OpenVR.System;

            TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            system.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevicePoses);

            for (int i = 0; i < trackedDevicePoses.Length; i++)
            {
                if (system.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.HMD)
                {
                    var matrix = trackedDevicePoses[i].mDeviceToAbsoluteTracking;

                    headPose.Position = new SharpDX.Vector3(matrix.m3, matrix.m7, matrix.m11);
                    headPose.Rotation = new SharpDX.Vector3(matrix.m0, matrix.m5, matrix.m10);

                    break;
                }
            }

            return headPose;
        }

        static Texture_t CreateTextureFromPng(string filePath)
        {
            Device device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            Texture2D texture = LoadTextureFromFile(device, filePath);

            Texture_t vrTexture = new Texture_t
            {
                handle = texture.NativePointer,
                eType = ETextureType.DirectX,
                eColorSpace = EColorSpace.Auto
            };

            return vrTexture;
        }

        static Texture2D LoadTextureFromFile(Device device, string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                var textureDesc = new Texture2DDescription
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R8G8B8A8_UNorm,
                    Height = image.Height,
                    Width = image.Width,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Immutable
                };

                GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                IntPtr dataPtr = handle.AddrOfPinnedObject();

                try
                {
                    var dataBox = new DataBox(dataPtr, image.Width * 4, 0);
                    return new Texture2D(device, textureDesc, new[] { dataBox });
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }

    public class Pose
    {
        public SharpDX.Vector3 Position { get; set; }
        public SharpDX.Vector3 Rotation { get; set; }
    }

    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float minValue, float maxValue)
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }
    }
}