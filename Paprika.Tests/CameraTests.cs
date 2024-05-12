using System.Numerics;

namespace Paprika.Tests;

using static VectorHelpers;

[TestClass]
public class CameraTests
{
    [TestMethod]
    public void TestCameraPos()
    {
        PaprikaCamera cam = new();
        Vector3 expected = new(1f, 2f, 3f);

        cam.Position = expected;

        Assert.AreEqual(expected, cam.Position);
    }



    [TestMethod]
    public void TestCameraRot()
    {
        PaprikaCamera cam = new();
        Quaternion expected = Quaternion.CreateFromYawPitchRoll(120f * DEG2RAD, 34f * DEG2RAD, 90f * DEG2RAD);

        cam.Rotation = expected;

        Assert.AreEqual(expected, cam.Rotation);
    }



    [TestMethod]
    public void TestCameraRotFromMatrix()
    {
        PaprikaCamera cam = new();
        Quaternion expected = Quaternion.CreateFromYawPitchRoll(120f * DEG2RAD, 34f * DEG2RAD, 90f * DEG2RAD);

        cam.Rotation = expected;

        Quaternion suspect = Quaternion.CreateFromRotationMatrix(cam.TRSMatrix);

        float epsilon = 0.00001f;

        Assert.AreEqual(expected.X, suspect.X, epsilon);
        Assert.AreEqual(expected.Y, suspect.Y, epsilon);
        Assert.AreEqual(expected.Z, suspect.Z, epsilon);
        Assert.AreEqual(expected.W, suspect.W, epsilon);
    }



    [TestMethod]
    public void TestCameraOrientating()
    {
        PaprikaCamera cam = new();
        Vector3 expectedPos = new(8f, 2f, 25f);
        Quaternion expectedRot = Quaternion.CreateFromYawPitchRoll(25f * DEG2RAD, 19.24f * DEG2RAD, -56.7583f * DEG2RAD);


        float epsilon = 0.00001f;


        cam.Rotation = expectedRot;
        cam.Position = expectedPos;


        Vector3 suspectPos = cam.TRSMatrix.Translation;
        Quaternion suspectRot = Quaternion.CreateFromRotationMatrix(cam.TRSMatrix);
        

        Assert.AreEqual(cam.Position, suspectPos);
        
        Assert.AreEqual(expectedRot.X, suspectRot.X, epsilon);
        Assert.AreEqual(expectedRot.Y, suspectRot.Y, epsilon);
        Assert.AreEqual(expectedRot.Z, suspectRot.Z, epsilon);
        Assert.AreEqual(expectedRot.W, suspectRot.W, epsilon);
    }



    [TestMethod]
    public void TestProjectionMatrix()
    {
        Vector3 inputPos = new(8f, 2f, 25f);
        Quaternion inputRot = Quaternion.CreateFromYawPitchRoll(25f * DEG2RAD, 19.24f * DEG2RAD, -56.7583f * DEG2RAD);
        Size2D inputSize = new(1920, 1200);
        PaprikaCamera cam = new (0f, 0f, 85.423f, default, inputPos, inputRot);
        ICamera<int> camInterface = cam;

        Matrix4x4 expected = Matrix4x4.CreatePerspectiveFieldOfView(cam.FOV.ToRadians(), inputSize.AspectRatio, camInterface.NearClip, camInterface.FarClip);

        Matrix4x4 actual = CameraHelpers.GetProjectionMatrix(cam, inputSize);



        Assert.AreEqual(expected, actual);
    }
}