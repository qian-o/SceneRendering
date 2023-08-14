﻿using Core.Tools;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Core.Contracts.Windows;

public abstract class Game
{
    private readonly List<double> _fpsSample;
    private readonly IWindow _window;

    protected GL gl = null!;
    protected IInputContext inputContext = null!;
    protected ImGuiController imGuiController = null!;
    protected Camera camera = null!;
    protected int fps;

    #region Input
    protected IMouse mouse = null!;
    protected IKeyboard keyboard = null!;
    protected bool firstMove = true;
    protected Vector2D<float> lastPos;
    #endregion

    #region Speeds
    protected float cameraSpeed = 4.0f;
    protected float cameraSensitivity = 0.2f;
    #endregion

    public Game()
    {
        _fpsSample = new List<double>();

        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        windowOptions.Samples = 8;
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 32;
        windowOptions.PreferredBitDepth = new Vector4D<int>(8);

        _window = Window.Create(windowOptions);
        _window.Load += () =>
        {
            imGuiController = new ImGuiController(gl = _window.CreateOpenGLES(), _window, inputContext = _window.CreateInput());
            camera = new Camera
            {
                Position = new Vector3D<float>(0.0f, 2.0f, 3.0f),
                Fov = 45.0f
            };

            mouse = inputContext.Mice[0];
            keyboard = inputContext.Keyboards[0];

            Load();
        };
        _window.Resize += Resize;
        _window.FramebufferResize += (obj) => { gl.Viewport(obj); FramebufferResize(obj); };
        _window.Update += (obj) =>
        {
            if (mouse.IsButtonPressed(MouseButton.Middle))
            {
                Vector2D<float> vector = new(mouse.Position.X, mouse.Position.Y);

                if (firstMove)
                {
                    lastPos = vector;

                    firstMove = false;
                }
                else
                {
                    float deltaX = vector.X - lastPos.X;
                    float deltaY = vector.Y - lastPos.Y;

                    camera.Yaw += deltaX * cameraSensitivity;
                    camera.Pitch += -deltaY * cameraSensitivity;

                    lastPos = vector;
                }
            }
            else
            {
                firstMove = true;
            }

            if (keyboard.IsKeyPressed(Key.W))
            {
                camera.Position += camera.Front * cameraSpeed * (float)obj;
            }

            if (keyboard.IsKeyPressed(Key.A))
            {
                camera.Position -= camera.Right * cameraSpeed * (float)obj;
            }

            if (keyboard.IsKeyPressed(Key.S))
            {
                camera.Position -= camera.Front * cameraSpeed * (float)obj;
            }

            if (keyboard.IsKeyPressed(Key.D))
            {
                camera.Position += camera.Right * cameraSpeed * (float)obj;
            }

            if (keyboard.IsKeyPressed(Key.Q))
            {
                camera.Position -= camera.Up * cameraSpeed * (float)obj;
            }

            if (keyboard.IsKeyPressed(Key.E))
            {
                camera.Position += camera.Up * cameraSpeed * (float)obj;
            }

            camera.Width = _window.Size.X;
            camera.Height = _window.Size.Y;

            Update(obj);
        };
        _window.Render += (obj) =>
        {
            if (_fpsSample.Count == 100)
            {
                fps = Convert.ToInt32(_fpsSample.Average());

                _fpsSample.Clear();
            }

            _fpsSample.Add(1.0d / obj);

            Render(obj);
        };
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
