using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

using static VectorHelpers;

public static class CameraHelpers
{
    public static void GetProjectionMatrix<TPixel>(
        in ICamera<TPixel> cam,
        in Size2D resolution,
        out Matrix4x4 projectionMatrix)
        
    where TPixel: unmanaged
    {
        projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(cam.FOV * DEG2RAD, resolution.AspectRatio, cam.NearClip, cam.FarClip);
    }



    public static Matrix4x4 GetProjectionMatrix<TPixel>(
        in ICamera<TPixel> cam,
        in Size2D resolution)
        
    where TPixel: unmanaged
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(cam.FOV.ToRadians(), resolution.AspectRatio, cam.NearClip, cam.FarClip);
    }



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
}