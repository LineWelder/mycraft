using System.Windows.Forms;

namespace Mycraft.Utils
{
    public class Input1d
    {
        public int Value { get; private set; }

        private readonly Keys up, down;

        public Input1d(Control control, Keys up, Keys down)
        {
            control.KeyDown += OnKeyDown;
            control.KeyUp += OnKeyUp;

            this.up = up;
            this.down = down;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == up   && Value ==  1
             || e.KeyCode == down && Value == -1)
                Value = 0;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if      (e.KeyCode == up)   Value = 1;
            else if (e.KeyCode == down) Value = -1;
        }
    }
}
