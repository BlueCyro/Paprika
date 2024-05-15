

using System.Numerics;
using Paprika;

string input = args.Length > 0 ? args[0] : "../Model/tinobed.glb";

int alignment = 16;
GLOutput output = new(1280, 1024, alignment);
SilkWindow window = new("Paprika Demo GL Output", 1280, 1024);

output.MainCamera.FOV = 60f;
// output.MainCamera.Position = new(0, 3, -4);
// output.MainCamera.Rotation = Quaternion.CreateFromYawPitchRoll(0f, 30f.ToRadians(), 0f);
output.MainCamera.Position = new(0.51144695f, 2.4718034f, 8.403356f);
output.MainCamera.Rotation =  new Quaternion(-0.019815441f, -0.9137283f, 0.40335205f, -0.044888653f);


DumbUploader geoReader = new();

if (!geoReader.Upload(input, alignment))
{
    Console.WriteLine("Failed to read geometry! Make sure the input file exists!");
    return;
}
GeometryHolder<TriangleWide>.UploadGeometry(geoReader.WideUploaded.Span, geoReader.WideUploaded.Length, alignment);


window.Output = output;
window.StartRender();