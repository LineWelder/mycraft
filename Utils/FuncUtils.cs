using System.Runtime.InteropServices;
using System.Windows.Forms;

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
    }
}
