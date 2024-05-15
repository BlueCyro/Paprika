using System.Numerics;

namespace Paprika;

using static VectorHelpers;

public interface ICamera<TPixel>

where TPixel: unmanaged

{
    // RenderBuffer<TPixel>? RenderBuffer { get; }
    Matrix4x4 TRSMatrix { get; }
    Matrix4x4 ViewMatrix { get; }
    Vector3 Position { get; }
    Quaternion Rotation { get; }
    virtual Vector3 Scale { get => Vector3.One; }
    virtual float FOV { get => -1f; }
    virtual float NearClip { get => 0.01f; }
    virtual float FarClip { get => 100f; }
}