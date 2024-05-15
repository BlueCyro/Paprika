using System.Numerics;
using System.Runtime.InteropServices;

namespace Paprika.Tests;


[TestClass]
public class RendererTests
{
    [TestMethod]
    public void RenderBufferAccessTest()
    {
        TestStruct testStruct = new();

        int expectedLength = 1280 * 1024;

        int suspect = testStruct.TestBuffer.Buffer.Length;


        Span<byte> suspectBytes = MemoryMarshal.Cast<int, byte>(testStruct.TestBuffer.Buffer.Span);


        Assert.AreEqual(expectedLength, suspect);
        // Assert.AreEqual(expectedLength, suspectBytes.Length);
    }
}


public struct TestStruct
{
    public RenderBuffer<int> TestBuffer;

    public TestStruct()
    {
        TestBuffer = new(1280, 1024);
    }
}