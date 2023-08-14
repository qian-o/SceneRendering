using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Core.Windows;

public class GameWindow
{
    private readonly IWindow _window;

    protected GL gl = null!;
    protected IInputContext inputContext = null!;
    protected ImGuiController imGuiController = null!;

    public GameWindow()
    {
        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        windowOptions.Samples = 8;
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 32;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        _window = Window.Create(windowOptions);
        _window.Load += () => { imGuiController = new ImGuiController(gl = _window.CreateOpenGLES(), _window, inputContext = _window.CreateInput()); Load(); };
        _window.Resize += Resize;
        _window.FramebufferResize += FramebufferResize;
        _window.Update += Update;
        _window.Render += Render;
        _window.Closing += Closing;
    }

    public void Run() => _window.Run();

    public void Close() => _window.Close();

    protected virtual void Load() { }

    protected virtual void Resize(Vector2D<int> obj) { }

    protected virtual void FramebufferResize(Vector2D<int> obj) { }

    protected virtual void Update(double obj) { }

    protected virtual void Render(double obj) { }

    protected virtual void Closing() { }
}
