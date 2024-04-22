using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
// [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct QuickColor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QuickColor(int rgba)
    {
        RGBA = rgba;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QuickColor(in byte r, in byte g, in byte b, in byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public QuickColor(in Vector3 rgb)
    {
        R = (byte)(rgb.X * 255f);
        G = (byte)(rgb.Y * 255f);
        B = (byte)(rgb.Z * 255f);
        A = 255;
    }



    [FieldOffset(0)]
    public byte R;
    
    [FieldOffset(1)]
    public byte G;
    
    [FieldOffset(2)]
    public byte B;
    
    [FieldOffset(3)]
    public byte A;

    [FieldOffset(0)]
    public int RGBA;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuickColor operator *(in QuickColor left, in QuickColor right)
    {
        return new QuickColor()
        {
            R = (byte)(left.R * right.R),
            G = (byte)(left.G * right.G),
            B = (byte)(left.B * right.B),
            A = (byte)(left.A * right.A)
        };
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuickColor operator *(in QuickColor left, in float right)
    {
        return new QuickColor()
        {
            R = (byte)(left.R * right),
            G = (byte)(left.G * right),
            B = (byte)(left.B * right),
        };
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigVec3 operator *(in QuickColor left, in Vector<float> right)
    {
        Vector<float> RWide = new Vector<float>(left.R) * right;
        Vector<float> GWide = new Vector<float>(left.G) * right;
        Vector<float> BWide = new Vector<float>(left.B) * right;

        return new(RWide, GWide, BWide);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuickColor operator +(in QuickColor left, in QuickColor right)
    {
        return new QuickColor()
        {
            R = (byte)(left.R + right.R),
            G = (byte)(left.G + right.G),
            B = (byte)(left.B + right.B),
        };
    }
}
