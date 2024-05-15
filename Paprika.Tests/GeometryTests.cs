
using System.Numerics;
using System.Runtime.InteropServices;

namespace Paprika.Tests;


[TestClass]
public class GeometryTests
{
    [TestMethod]
    public void GeometryUploadAccessTest()
    {
        DumbUploader dumb = new();
        if (!dumb.Upload("./Model/tinobed.glb"))
            Assert.Fail("Failed to upload model");

        GeometryHolder<TriangleWide>.UploadGeometry(dumb.WideUploaded.Span, dumb.WideUploaded.Length);

        var truth = dumb.singles;

        var suspect = GeometryHolder<TriangleWide>.Geometry.Span;

        if (truth.Count >= (suspect.Length * Vector<float>.Count))
            Assert.Fail($"Uploaded triangle length is not greater than or equal to the original list. Truth: {truth.Count}, Actual: {suspect.Length}");


        for (int i = 0; i < truth.Count; i++)
        {
            int wideIndex = i / Vector<float>.Count;
            TriangleWide.ReadSlot(ref suspect[wideIndex], i % 8, out Triangle narrowSuspect);
            
            Assert.AreEqual(truth[i], narrowSuspect);
        }
    }
}