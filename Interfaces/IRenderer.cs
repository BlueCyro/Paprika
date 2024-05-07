namespace Paprika;

public interface IRenderer
{
    public Camera MainCamera { get; }
    public void RenderFrame();
}
