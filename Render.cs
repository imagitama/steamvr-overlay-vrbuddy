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

        public Render(ref Settings settings)
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

        public void RenderTrackingData(TrackingData trackingData, SimpleVector3 positionOffset, SimpleQuaternion rotationOffset, bool lookAtPlayer)
        {
            SetOverlayTransform(headOverlayHandle, trackingData.Hmd, positionOffset, rotationOffset, lookAtPlayer);
            SetOverlayTransform(leftHandOverlayHandle, trackingData.LeftHand, positionOffset, rotationOffset, lookAtPlayer);
            SetOverlayTransform(rightHandOverlayHandle, trackingData.RightHand, positionOffset, rotationOffset, lookAtPlayer);

            OpenVR.Overlay.SetOverlayTexture(headOverlayHandle, ref textureHead);
            OpenVR.Overlay.SetOverlayTexture(leftHandOverlayHandle, ref textureLeftHand);
            OpenVR.Overlay.SetOverlayTexture(rightHandOverlayHandle, ref textureRightHand);
        }

        void SetOverlayTransform(ulong overlayHandle, Transform pose, SimpleVector3 positionOffset, SimpleQuaternion rotationOffset, bool lookAtPlayer)
        {
            var transform = new HmdMatrix34_t();

            transform.m3 = pose.Position.X + positionOffset.X;
            transform.m7 = pose.Position.Y + positionOffset.Y;
            transform.m11 = pose.Position.Z + positionOffset.Z;

            Quaternion combinedRotation;

            if (lookAtPlayer)
            {
                // TODO
                combinedRotation = pose.Rotation.ToSharpDXQuaternion();
            }
            else
            {
                Quaternion poseRotation = Quaternion.RotationYawPitchRoll(pose.Rotation.Y, pose.Rotation.X, pose.Rotation.Z);
                combinedRotation = Quaternion.Multiply(poseRotation, rotationOffset.ToSharpDXQuaternion());
            }

            var rotationMatrix = Matrix.RotationQuaternion(combinedRotation);
    
            transform.m0 = rotationMatrix.M11;
            transform.m1 = rotationMatrix.M12;
            transform.m2 = rotationMatrix.M13;
            transform.m4 = rotationMatrix.M21;
            transform.m5 = rotationMatrix.M22;
            transform.m6 = rotationMatrix.M23;
            transform.m8 = rotationMatrix.M31;
            transform.m9 = rotationMatrix.M32;
            transform.m10 = rotationMatrix.M33;
            
            SetOverlayTransformAbsolute(overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref transform);
        }

        Quaternion LookRotation(Vector3 direction)
        {
            float dot = Vector3.Dot(new Vector3(0, 0, 1), direction);
            if (MathF.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(0, 1, 0, MathF.PI);
            }
            if (MathF.Abs(dot - 1.0f) < 0.000001f)
            {
                return Quaternion.Identity;
            }

            float rotAngle = MathF.Acos(dot);
            Vector3 rotAxis = Vector3.Cross(new Vector3(0, 0, 1), direction);
            rotAxis.Normalize();
            return Quaternion.RotationAxis(rotAxis, rotAngle);
        }

        // virtual so we can have a virtual playspace later
        public virtual void SetOverlayTransformAbsolute(ulong ulOverlayHandle, ETrackingUniverseOrigin eTrackingOrigin, ref HmdMatrix34_t pmatTrackingOriginToOverlayTransform) {
            OpenVR.Overlay.SetOverlayTransformAbsolute(ulOverlayHandle, eTrackingOrigin, ref pmatTrackingOriginToOverlayTransform);
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

    public static class RandomExtensions
    {
        public static float NextFloat(this Random random, float minValue, float maxValue)
        {
            return (float)(random.NextDouble() * (maxValue - minValue) + minValue);
        }
    }
}