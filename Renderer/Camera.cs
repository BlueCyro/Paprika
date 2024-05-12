using System.Numerics;
using System.Runtime.InteropServices;

namespace Paprika;

using static VectorHelpers;

// [StructLayout(LayoutKind.Sequential, Pack = 64)]
public class Camera(float nearClip, float farClip, float fov, Size2D resolution, Vector3 position, Quaternion rotation)
{
    public Camera(float nearClip, float farClip, float fov, Size2D resolution, Vector3 position, Quaternion rotation, bool init = true) : this(nearClip, farClip, fov, resolution, position, rotation)
    {
        if (init)
        {
            UpdateProjectionMatrix();
            UpdateViewMatrix();
        }
    }



    public Vector3 Forward { get; private set; }
    public Vector3 Up { get; private set; }
    public Vector3 Right { get; private set; }
    public Matrix4x4 ViewProjectionMatrix => _viewMatrix * _projectionMatrix;



    public float NearClip
    {
        get => nearClip;
        set
        {
            nearClip = value;
            UpdateProjectionMatrix();
        }
    }



    public float FarClip
    {
        get => farClip;
        set
        {
            farClip = value;
            UpdateProjectionMatrix();
        }
    }



    public Size2D Resolution
    {
        get => resolution;
        set
        {
            resolution = value;
            UpdateProjectionMatrix();
        }
    }



    public float FOV
    {
        get => fov;
        set
        {
            fov = value;
            UpdateProjectionMatrix();
        }
    }


    
    public Vector3 Position
    {
        get => position;
        set
        {
            position = value;
            UpdateViewMatrix();
        }
    }



    public Quaternion Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            UpdateViewMatrix();
        }
    }
    


    internal Matrix4x4 _viewMatrix;
    internal Matrix4x4 _projectionMatrix;


    
    private void UpdateProjectionMatrix()
    {
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fov * DEG2RAD, Resolution.AspectRatio, nearClip, farClip);
    }



    private void UpdateViewMatrix()
    {
        Forward = Vector3.Transform(Vector3.UnitZ, rotation);
        Up = Vector3.Transform(Vector3.UnitY, rotation);
        Right = Vector3.Transform(Vector3.UnitX, rotation);
        _viewMatrix = Matrix4x4.CreateLookAt(position, position + Forward, Up);
    }
}