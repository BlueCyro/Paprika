using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

using static VectorHelpers;

public static class CameraHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToDegrees(this float radians)
    {
        return radians * RAD2DEG;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToRadians(this float degrees)
    {
        return degrees * DEG2RAD;
    }


    public static void GetPerspectiveProjection<TCamera, TPixel>(
        in TCamera cam,
        in Size2D resolution,
        out Matrix4x4 projectionMatrix)

    where TPixel: unmanaged
    where TCamera: struct, ICamera<TPixel>
    {
        projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(cam.FOV.ToRadians(), resolution.AspectRatio, cam.NearClip, cam.FarClip);
    }



    public static Matrix4x4 GetPerspectiveProjection<TCamera, TPixel>(
        in TCamera cam,
        in Size2D resolution)

    where TPixel: unmanaged
    where TCamera: struct, ICamera<TPixel>
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(cam.FOV.ToRadians(), resolution.AspectRatio, cam.NearClip, cam.FarClip);
    }
}