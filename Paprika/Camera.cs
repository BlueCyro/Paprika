using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

public struct PaprikaCamera : ICamera<int>
{
    public PaprikaCamera(float nearClip, float farClip, float fov, Size2D resolution, Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
        FOV = fov;
    }



    public Matrix4x4 ViewMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        private set;
    }
    
    
    
    public Matrix4x4 TRSMatrix { get; set; }



    public Quaternion Rotation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => rotation;
        set
        {
            // Quaternion.CreateFromRotationMatrix(trsMatrix);
            rotation = value;
            UpdateViewMatrix();
        }
    }
    Quaternion rotation = Quaternion.Identity;



    public Vector3 Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => position;
        set
        {
            position = value;
            UpdateViewMatrix();
        }
    }
    Vector3 position;


    
    public float FOV { get; set; }



    public readonly Vector3 Up => Vector3.Transform(Vector3.UnitY, rotation);



    public readonly Vector3 Right => Vector3.Transform(Vector3.UnitX, rotation);



    public readonly Vector3 Forward => Vector3.Transform(Vector3.UnitZ, rotation);



    private void UpdateViewMatrix()
    {
        // Matrix4x4 rotMat = Matrix4x4.CreateFromQuaternion(Rotation);

        // Vector3 forward = Vector3.Transform(Vector3.UnitZ, Rotation);
        // Vector3 up = Vector3.Transform(Vector3.UnitY, Rotation);
        // Vector3 right = Vector3.Transform(Vector3.UnitX, rotMat);
        ViewMatrix = Matrix4x4.CreateLookAt(Position, Position + Forward, Up);

        // Matrix4x4 rotMatrix = Matrix4x4.CreateFromQuaternion(rotation);

        // Matrix4x4 transMatrix = Matrix4x4.CreateTranslation(position);

        // ViewMatrix = transMatrix * rotMatrix;
    }
}
