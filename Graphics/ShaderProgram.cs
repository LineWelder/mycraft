using OpenGL;
using System;
using System.Text;

namespace Mycraft.Graphics
{
    public class ShaderProgram : IDisposable
    {
        public readonly uint glId;

        public ShaderProgram(string vertexSource, string fragmentSource)
        {
            glId = Gl.CreateProgram();

            uint vertex = CreateShader(ShaderType.VertexShader, vertexSource);
            uint fragment = CreateShader(ShaderType.FragmentShader, fragmentSource);

            Gl.AttachShader(glId, vertex);
            Gl.AttachShader(glId, fragment);
            Gl.LinkProgram(glId);

            Gl.GetProgram(glId, ProgramProperty.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                Gl.GetProgram(glId, ProgramProperty.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(glId, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            Gl.DeleteShader(vertex);
            Gl.DeleteShader(fragment);
        }

        public void Dispose() => Gl.DeleteProgram(glId);

        private uint CreateShader(ShaderType shaderType, string source)
        {
            uint id = Gl.CreateShader(shaderType);
            Gl.ShaderSource(id, new string[] { source });
            Gl.CompileShader(id);

            Gl.GetShader(id, ShaderParameterName.CompileStatus, out int сompileStatus);
            if (сompileStatus == 0)
            {
                Gl.GetShader(id, ShaderParameterName.InfoLogLength, out int infoLogLength);

                StringBuilder errorString = new StringBuilder(infoLogLength);
                Gl.GetShaderInfoLog(id, infoLogLength, out _, errorString);

                throw new InvalidOperationException($"Shader compiling error: {errorString}");
            };

            return id;
        }

        protected int FindVariable(string name)
        {
            int location = Gl.GetUniformLocation(glId, name);
            if (location < 0)
                throw new InvalidOperationException($"{name} variable not found");

            return location;
        }
    }
}
