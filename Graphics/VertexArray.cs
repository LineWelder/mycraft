using OpenGL;
using System;

namespace Mycraft.Graphics
{
    public class VertexArray : IDisposable
    {
        public readonly uint glId;

        private readonly int verticesCount;
        private readonly uint vbo;

        public VertexArray(float[] vertices)
        {
            if (vertices.Length % 3 != 0)
                throw new ArgumentException("Invalid vertices data");

            verticesCount = vertices.Length / 3;
    
            glId = Gl.GenVertexArray();
            Gl.BindVertexArray(glId);

            vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * vertices.Length), vertices, BufferUsage.StaticDraw);

            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(0);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        public void Draw(PrimitiveType primitiveType)
        {
            Gl.BindVertexArray(glId);
            Gl.DrawArrays(primitiveType, 0, verticesCount);
        }

        public void Dispose()
        {
            Gl.DeleteBuffers(vbo);
            Gl.DeleteVertexArrays(glId);
        }
    }
}
