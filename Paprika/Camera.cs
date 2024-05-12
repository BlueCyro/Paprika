using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

using static VectorHelpers;

public struct PaprikaCamera : ICamera<int>
{
    public PaprikaCamera()
    {
        trsMatrix = Matrix4x4.Identity;
    }



    public PaprikaCamera(float nearClip, float farClip, float fov, Size2D resolution, Vector3 position, Quaternion rotation) : this()
    {
        trsMatrix = Matrix4x4.Transform(trsMatrix, rotation);
        trsMatrix.Translation = position;
        FOV = fov;
    }
    


    // public RenderBuffer<int>? RenderBuffer { get; set; }
    
    
    public readonly Matrix4x4 ViewProjectionMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => LookAtMatrix * ProjectionMatrix;
    }



    public Vector3 Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => trsMatrix.Translation;
        set
        {
            trsMatrix.Translation = value;
            UpdateViewMatrix();
        }
    }



    public Matrix4x4 LookAtMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        private set;
    }



    public Matrix4x4 ProjectionMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        private set;
    }
    
    
    
    public Matrix4x4 TRSMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => trsMatrix;
        set => trsMatrix = value;
    }
    Matrix4x4 trsMatrix;



    public Quaternion Rotation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => rotation;
        set
        {
            // Quaternion.CreateFromRotationMatrix(trsMatrix);
            rotation = value;
            trsMatrix = Matrix4x4.Transform(trsMatrix, value);
        }
    }
    Quaternion rotation;


    
    public float FOV { get; set; }



    private void UpdateViewMatrix()
    {
        Matrix4x4 rotMat = Matrix4x4.CreateFromQuaternion(Rotation);

        Vector3 forward = Vector3.Transform(Vector3.UnitZ, rotMat);
        Vector3 up = Vector3.Transform(Vector3.UnitY, rotMat);
        // Vector3 right = Vector3.Transform(Vector3.UnitX, rotMat);
        LookAtMatrix = Matrix4x4.CreateLookAt(Position, forward, up);
    }
}



public struct PaprikaRenderer : IRenderer<int>
{
    public RenderBuffer<int> OutputBuffer { get; private set; }
    public ICamera<int> MainCamera { get; private set; }
    public readonly bool CanUploadGeometry => true;



    public void RenderFrame()
    {

    }


}