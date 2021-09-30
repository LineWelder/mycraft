using System;
using System.Diagnostics;
using System.Linq;
using OpenGL;

namespace Mycraft.Graphics
{
    public class VertexArray : IDisposable
    {
        public readonly uint glId;

        protected float[] Data
        {
            set
            {
                Debug.Assert(value.Length % vertexSize == 0, "Invalid vertices data");
                verticesCount = value.Length / vertexSize;

                Gl.BindVertexArray(glId);
                Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * value.Length), value, BufferUsage.StaticDraw);

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
            }
        }

        private readonly PrimitiveType primitiveType;
        private readonly int[] vertexFormat;
        private readonly int vertexSize;
        private readonly uint vbo;

        private int verticesCount;

        /// <param name="vertexFormat">The index is the variable location and the value is the variable size</param>
        public VertexArray(PrimitiveType primitiveType, int[] vertexFormat)
        {
            this.primitiveType = primitiveType;
            this.vertexFormat = vertexFormat;
            vertexSize = vertexFormat.Sum();

            glId = Gl.GenVertexArray();
            Gl.BindVertexArray(glId);

            vbo = Gl.GenBuffer();
        }

        public VertexArray(PrimitiveType primitiveType, int[] vertexFormat, float[] data)
            : this(primitiveType, vertexFormat)
        {
            Data = data;
        }

        public void Draw()
        {
            if (verticesCount == 0) return;

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
