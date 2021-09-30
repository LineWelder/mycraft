using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenGL;

namespace Mycraft.Utils
{
    public static class FuncUtils
    {
        [DllImport("user32.dll")]
        private static extern short GetKeyState(int key);

        public static bool IsKeyPressed(Keys key)
            => (GetKeyState((int)key) & 128 ) != 0;

        public static int GetKeyPressed(Keys key)
            => ( GetKeyState((int)key) & 128 ) >> 7;

        public static int GetInput1d(Keys up, Keys down)
            => GetKeyPressed(up) - GetKeyPressed(down);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4f TranslateBy(Vertex3f vector)
            => Matrix4x4f.Translated(
                vector.x,
                vector.y,
                vector.z
            );

        public static float FixRotation(float val)
        {
            const float TWO_PI = 2f * (float)Math.PI;

            if (val > TWO_PI)
                val -= TWO_PI;
            else if (val < 0)
                val += TWO_PI;

            return val;
        }

        public static float Clamp(float min, float val, float max)
        {
            if (val > max)
                return max;

            if (val < min)
                return min;

            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBetween(float min, float val, float max)
            => min < val && val < max;
    }
}
