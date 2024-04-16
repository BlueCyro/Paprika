using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct QuickColor(in byte r, in byte g, in byte b, in byte a)
{
    [FieldOffset(0)]
    public byte R = r;
    
    [FieldOffset(1)]
    public byte G = g;
    
    [FieldOffset(2)]
    public byte B = b;
    
    [FieldOffset(3)]
    public byte A = a;

    [FieldOffset(0)]
    public int RGBA;


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



    public static QuickColor operator *(in QuickColor left, in float right)
    {
        return new QuickColor()
        {
            R = (byte)(left.R * right),
            G = (byte)(left.G * right),
            B = (byte)(left.B * right),
        };
    }



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
