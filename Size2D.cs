using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Silk.NET.Maths;

namespace Paprika;

[StructLayout(LayoutKind.Explicit, Pack = 32)]
public readonly struct Size2D // There are way too many things that are just a 2D vector, man.
{
    public Size2D(in int width, in int height)
    {
        Width = width;
        Height = height;
        WidthSingle = width;
        HeightSingle = height;
    }



    public Size2D(in float width, in float height)
    {
        Width = (int)width;
        Height = (int)height;
        WidthSingle = Width;
        HeightSingle = Height;
    }


    [FieldOffset(0)]
    public readonly uint UWidth;

    [FieldOffset(4)]
    public readonly uint UHeight;


    [FieldOffset(0)]
    public readonly int Width;

    [FieldOffset(4)]
    public readonly int Height;

    [FieldOffset(0)]
    public readonly Vector64<int> Size64;

    [FieldOffset(0)]
    public readonly Vector2D<int> Size;



    [FieldOffset(8)]
    public readonly float WidthSingle;
    
    [FieldOffset(12)]
    public readonly float HeightSingle;

    [FieldOffset(8)]
    public readonly Vector64<float> Size64Single;

    [FieldOffset(8)]
    public readonly Vector2 SizeVector2;



    public readonly int Length1D => Width * Height;
    public readonly float AspectRatio => WidthSingle / HeightSingle;


    
    public static implicit operator Size2D(in Vector64<int> v64) => new(v64[0], v64[1]);

    public static implicit operator Size2D(in Vector2D<int> v2D) => new(v2D.X, v2D.Y);

    public static implicit operator Size2D(in Vector64<float> v64F) => new(v64F[0], v64F[1]);

    public static implicit operator Size2D(in Vector2 v2DF) => new(v2DF.X, v2DF.Y);



    public static implicit operator Vector64<int>(in Size2D size) => size.Size64;

    public static implicit operator Vector2D<int>(in Size2D size) => size.Size;

    public static implicit operator Vector64<float>(in Size2D size) => size.Size64Single;

    public static implicit operator Vector2(in Size2D size) => size.SizeVector2;
}