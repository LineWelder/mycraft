using OpenGL;
using System;
using System.Diagnostics;
using System.Linq;

namespace Mycraft.Graphics
{
    public class VertexArray : IDisposable
    {
        public readonly uint glId;

        private readonly PrimitiveType primitiveType;
        private readonly int verticesCount;
        private readonly uint vbo;

        /// <param name="vertexFormat">The index is the variable location and the value is the variable size</param>
        public VertexArray(PrimitiveType primitiveType, float[] data, int[] vertexFormat)
        {
            int vertexSize = vertexFormat.Sum();
            Debug.Assert(data.Length % vertexSize == 0, "Invalid vertices data");

            this.primitiveType = primitiveType;
            verticesCount = data.Length / vertexSize;
    
            glId = Gl.GenVertexArray();
            Gl.BindVertexArray(glId);

            vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * data.Length), data, BufferUsage.StaticDraw);

            int offset = 0;
            for (uint i = 0; i < vertexFormat.Length; i++)
            {
                Debug.Assert(vertexFormat[i] > 0, "Variable size must be greater than zero");

                Gl.VertexAttribPointer(
                    i, vertexFormat[i],
                    VertexAttribType.Float, false,
                    sizeof(float) * vertexSize,
                    IntPtr.Zero + sizeof(float) * offset
                );
                Gl.EnableVertexAttribArray(i);
                offset += vertexFormat[i];
            }

            Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        public void Draw()
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
