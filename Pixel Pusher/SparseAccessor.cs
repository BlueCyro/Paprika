using System.Runtime.CompilerServices;


namespace Paprika;

public readonly ref struct SparseAccessor<T>(ref T first, int stride, int padding)
{
    readonly ref T firstElement = ref first;
    
    public readonly ref T ElementAt(in int index, out int offset)
    {
        offset = (index % stride) + index / stride * padding;
        return ref Unsafe.Add(ref firstElement, offset);
    }
}