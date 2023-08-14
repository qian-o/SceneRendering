using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Tools;

public unsafe class Program : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, int> _attribLocations;
    private readonly Dictionary<string, int> _uniformLocations;

    private Shader? currentVert;
    private Shader? currentFrag;

    public uint Id { get; }

    public Program(GL gl)
    {
        _gl = gl;
        _attribLocations = new Dictionary<string, int>();
        _uniformLocations = new Dictionary<string, int>();

        Id = _gl.CreateProgram();
    }

    public void Attach(Shader vs, Shader fs)
    {
        if (vs != null && currentVert != vs)
        {
            if (currentVert != null)
            {
                _gl.DetachShader(Id, currentVert.Id);
            }

            _gl.AttachShader(Id, vs.Id);

            currentVert = vs;
        }

        if (fs != null && currentFrag != vs)
        {
            if (currentFrag != null)
            {
                _gl.DetachShader(Id, currentFrag.Id);
            }

            _gl.AttachShader(Id, fs.Id);

            currentFrag = fs;
        }

        _gl.LinkProgram(Id);

        string error = _gl.GetProgramInfoLog(Id);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Program:{Id}, Error:{error}");
        }
    }

    public void Enable()
    {
        _gl.UseProgram(Id);
    }

    public void Disable()
    {
        _gl.UseProgram(0);
    }

    public int GetAttrib(string name)
    {
        if (!_attribLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetAttribLocation(Id, name);

            _attribLocations[name] = value;
        }

        return value;
    }

    public int GetUniform(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetUniformLocation(Id, name);

            _uniformLocations[name] = value;
        }

        return value;
    }

    public void EnableAttrib(string name)
    {
        Enable();

        _gl.EnableVertexAttribArray((uint)GetAttrib(name));
    }

    public void DisableAttrib(string name)
    {
        Enable();

        _gl.DisableVertexAttribArray((uint)GetAttrib(name));
    }

    public void SetUniform(string name, int data)
    {
        Enable();

        _gl.Uniform1(GetUniform(name), data);
    }

    public void SetUniform(string name, float data)
    {
        Enable();

        _gl.Uniform1(GetUniform(name), data);
    }

    public void SetUniform(string name, double data)
    {
        Enable();

        _gl.Uniform1(GetUniform(name), Convert.ToSingle(data));
    }

    public void SetUniform(string name, Vector2D<float> data)
    {
        Enable();

        _gl.Uniform2(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Vector3D<float> data)
    {
        Enable();

        _gl.Uniform3(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Vector4D<float> data)
    {
        Enable();

        _gl.Uniform4(GetUniform(name), 1, (float*)&data);
    }

    public void SetUniform(string name, Matrix2X2<float> data)
    {
        Enable();

        _gl.UniformMatrix2(GetUniform(name), 1, false, (float*)&data);
    }

    public void SetUniform(string name, Matrix3X3<float> data)
    {
        Enable();

        _gl.UniformMatrix3(GetUniform(name), 1, false, (float*)&data);
    }

    public void SetUniform(string name, Matrix4X4<float> data)
    {
        Enable();

        _gl.UniformMatrix4(GetUniform(name), 1, false, (float*)&data);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);
        _attribLocations.Clear();
        _uniformLocations.Clear();

        GC.SuppressFinalize(this);
    }
}