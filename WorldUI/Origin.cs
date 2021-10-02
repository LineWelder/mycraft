using System;
using OpenGL;

using Mycraft.Graphics;
using Mycraft.Utils;

namespace Mycraft.WorldUI
{
    public class Origin : IDisposable
    {
        private static readonly float[] xAxisVertices =
        {
            0f, 0f, 0f,  1f, 0f, 0f,
            1f, 0f, 0f,  1f, 0f, 0f
        };

        private static readonly float[] yAxisVertices =
        {
            0f, 0f, 0f,  0f, 1f, 0f,
            0f, 1f, 0f,  0f, 1f, 0f
        };

        private static readonly float[] zAxisVertices =
        {
            0f, 0f, 0f,  0f, 0f, 1f,
            0f, 0f, 1f,  0f, 0f, 1f
        };

        private VertexArray xAxis, yAxis, zAxis;

        public Origin()
        {
            xAxis = new VertexArray(PrimitiveType.Lines, Resources.WorldUIShader, xAxisVertices);
            yAxis = new VertexArray(PrimitiveType.Lines, Resources.WorldUIShader, yAxisVertices);
            zAxis = new VertexArray(PrimitiveType.Lines, Resources.WorldUIShader, zAxisVertices);
        }

        public void Draw()
        {
            Resources.WorldUIShader.Model = Matrix4x4f.Identity;
            Resources.WorldUIShader.Color = new Vertex3f(1f, 0f, 0f);
            xAxis.Draw();
            Resources.WorldUIShader.Color = new Vertex3f(0f, 1f, 0f);
            yAxis.Draw();
            Resources.WorldUIShader.Color = new Vertex3f(0f, 0f, 1f);
            zAxis.Draw();
        }

        public void Dispose()
        {
            xAxis.Dispose();
            yAxis.Dispose();
            zAxis.Dispose();
        }
    }
}
