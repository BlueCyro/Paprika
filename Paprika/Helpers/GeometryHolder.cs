namespace Paprika;



public static class GeometryHolder<TGeo> where TGeo: unmanaged, IGeometry
{
    public static DumbBuffer<TGeo> Geometry { get; private set; }




    public static void UploadGeometry(Span<TGeo> geos, int count, int alignment = 32) // TODO: Make this a little more useful perhaps?
    {
        Geometry = new(count, alignment);

        geos.CopyTo(Geometry.Span);
    }
}
