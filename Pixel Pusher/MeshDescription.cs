using System.Numerics;

namespace Paprika;


public class MeshDescription(string name)
{
    public string Name = name;
    public List<MeshVertex> Vertices = [];
}



public class MeshVertex(Vector3 pos, Vector3 norm, VertexType type, int index)
{
    public readonly Vector3 Position = pos;
    public readonly Vector3 Normal = norm;
    public readonly VertexType Type = type;
    public readonly int MaterialIndex = index;
    public readonly List<IMeshAttribute> Attributes = [];
}


public class VertexAttribute<T>(string name, T val) : IMeshAttribute where T: unmanaged
{
    public string Name { get; private set; } = name ?? "";
    public Type AttributeType => typeof(T);
    public object BoxedValue => Value;
    public readonly T Value = val;
}



public interface IMeshAttribute
{
    public string Name { get; }
    public Type AttributeType { get; }
    public object BoxedValue { get; }
}



public struct Vertex(in Vector3 pos, in Vector3 normal, in Vector3 tangent, in Vector2 uv)
{
    public Vector3 Position = pos;
    public Vector3 Normal = normal;
    public Vector3 Tangent = tangent;

    public Vector2 UV = uv;
}






public interface IVertex
{
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public int Index { get; }
}



public interface IVertexAttribute
{

}



public enum VertexType : byte
{
    Triangle,
    Point
}
