using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepuUtilities;


namespace Paprika;

// [StructLayout(LayoutKind.Sequential, Size = 512 )]
public struct Matrix4x4Wide
{
    public Vector4Wide X;
    public Vector4Wide Y;
    public Vector4Wide Z;
    public Vector4Wide W;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Broadcast(in Matrix4x4 from, out Matrix4x4Wide widened)
    {
        widened.X.X = new(from.M11);
        widened.X.Y = new(from.M12);
        widened.X.Z = new(from.M13);
        widened.X.W = new(from.M14);

        widened.Y.X = new(from.M21);
        widened.Y.Y = new(from.M22);
        widened.Y.Z = new(from.M23);
        widened.Y.W = new(from.M24);

        widened.Z.X = new(from.M31);
        widened.Z.Y = new(from.M32);
        widened.Z.Z = new(from.M33);
        widened.Z.W = new(from.M34);

        widened.W.X = new(from.M41);
        widened.W.Y = new(from.M42);
        widened.W.Z = new(from.M43);
        widened.W.W = new(from.M44);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateIdentity(out Matrix4x4Wide identity)
    {
        identity.X.X = Vector<float>.One;
        identity.X.Y = Vector<float>.Zero;
        identity.X.Z = Vector<float>.Zero;
        identity.X.W = Vector<float>.Zero;

        identity.Y.X = Vector<float>.Zero;
        identity.Y.Y = Vector<float>.One;
        identity.Y.Z = Vector<float>.Zero;
        identity.Y.W = Vector<float>.Zero;

        identity.Z.X = Vector<float>.Zero;
        identity.Z.Y = Vector<float>.Zero;
        identity.Z.Z = Vector<float>.One;
        identity.Z.W = Vector<float>.Zero;

        identity.W.X = Vector<float>.Zero;
        identity.W.Y = Vector<float>.Zero;
        identity.W.Z = Vector<float>.Zero;
        identity.W.W = Vector<float>.One;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Transform(in Vector4Wide vector, in Matrix4x4Wide transform, out Vector4Wide transformed)
    {
        Vector4Wide.Scale(transform.X, vector.X, out Vector4Wide transformed1);
        Vector4Wide.Scale(transform.Y, vector.Y, out Vector4Wide transformed2);
        Vector4Wide.Scale(transform.Z, vector.Z, out Vector4Wide transformed3);
        Vector4Wide.Scale(transform.W, vector.W, out Vector4Wide transformed4);

        transformed = transformed1 + transformed2 + transformed3 + transformed4;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Transform(in Vector3Wide vector, in Matrix4x4Wide transform, out Vector4Wide transformed)
    {
        Vector4Wide.Scale(transform.X, vector.X, out Vector4Wide transformed1);
        Vector4Wide.Scale(transform.Y, vector.Y, out Vector4Wide transformed2);
        Vector4Wide.Scale(transform.Z, vector.Z, out Vector4Wide transformed3);

        transformed = transformed1 + transformed2 + transformed3 + transform.W;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Transform(in Vector3Wide vector, in Matrix4x4Wide transform, out Vector3Wide transformed)
    {
        Vector4Wide.Scale(transform.X, vector.X, out Vector4Wide transformed1);
        Vector4Wide.Scale(transform.Y, vector.Y, out Vector4Wide transformed2);
        Vector4Wide.Scale(transform.Z, vector.Z, out Vector4Wide transformed3);

        Vector4Wide transform4 = transformed1 + transformed2 + transformed3 + transform.W;
        transformed = *(Vector3Wide*)&transform4;
        // transformed = (*(Vector3Wide*)&transformed1) + (*(Vector3Wide*)&transformed2) + (*(Vector3Wide*)&transformed3) + &transform->W;
    }
}