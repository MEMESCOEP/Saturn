/* DIRECTIVES */
using System.Numerics;
using Hexa.NET.Raylib;


/* NAMESPACES */
namespace Saturn
{
    /* CLASSES */
    class CameraUtils
    {
        /* FUNCTIONS */
        // Requires "ref" for the camera because going from Editor -> ViewportUtils doesn't create a copy, but going from Editor -> CameraUtils -> ViewportUtils does create one
        public static void RotateCameraAngleAxis(ref Camera3D Camera, Vector3 RotationAxis, float Angle, bool RotateAroundTarget = false)
        {
            ConsoleUtils.StatusWrite($"Rotating camera by {RotationAxis * Angle}...");
            Raylib.CameraPitch(ref Camera, RotationAxis.Y * (Angle * Raylib_cs.Raylib.DEG2RAD), false, RotateAroundTarget, true);
            Raylib.CameraRoll(ref Camera, RotationAxis.Z * (Angle * Raylib_cs.Raylib.DEG2RAD));
            Raylib.CameraYaw(ref Camera, -RotationAxis.X * (Angle * Raylib_cs.Raylib.DEG2RAD), RotateAroundTarget);
        }

        // Requires "ref" for the camera because going from Editor -> ViewportUtils doesn't create a copy, but going from Editor -> CameraUtils -> ViewportUtils does create one
        public static void RotateCameraVector3(ref Camera3D Camera, Vector3 NewRotation, bool RotateAroundTarget = false)
        {
            Vector3 CurrentRotation = Raylib_cs.Raymath.QuaternionToEuler(Raylib_cs.Raymath.QuaternionFromVector3ToVector3(Camera.Position, Camera.Target));
            Vector3 Guh = Raylib_cs.Raymath.Vector3Subtract(CurrentRotation, NewRotation);
            ConsoleUtils.StatusWrite($"{CurrentRotation}, ({Guh.X * Raylib_cs.Raylib.RAD2DEG}, {Guh.Y * Raylib_cs.Raylib.RAD2DEG}, {Guh.Z * Raylib_cs.Raylib.RAD2DEG})");

            Raylib.CameraPitch(ref Camera, (CurrentRotation.Y - NewRotation.Y) * Raylib_cs.Raylib.DEG2RAD, false, RotateAroundTarget, true);
            Raylib.CameraRoll(ref Camera, (CurrentRotation.Z - NewRotation.Z) * Raylib_cs.Raylib.DEG2RAD);
            Raylib.CameraYaw(ref Camera, -(CurrentRotation.X - NewRotation.X) * Raylib_cs.Raylib.DEG2RAD, RotateAroundTarget);
        }
    }
}