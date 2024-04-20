using System.Runtime.InteropServices;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
public unsafe ref struct TriangleBanger<T>(in T data) where T : struct
{
    [FieldOffset(0)]
    public T FragmentData = data;
}
