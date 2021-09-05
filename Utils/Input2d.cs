using System.Windows.Forms;

namespace Mycraft.Utils
{
    public class Input2d
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        private readonly Keys up, left, down, right;

        public Input2d(Control control, Keys up, Keys left, Keys down, Keys right)
        {
            control.KeyDown += OnKeyDown;
            control.KeyUp += OnKeyUp;

            this.up = up;
            this.left = left;
            this.down = down;
            this.right = right;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == up   && Y ==  1
             || e.KeyCode == down && Y == -1)
                Y = 0;
            else if (e.KeyCode == right && X ==  1
                  || e.KeyCode == left  && X == -1)
                X = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if      (e.KeyCode == up)    Y = 1;
            else if (e.KeyCode == down)  Y = -1;
            else if (e.KeyCode == right) X = 1;
            else if (e.KeyCode == left)  X = -1;
        }
    }
}
