using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenGL;
using Mycraft.Utils;

namespace Mycraft
{
    public class GameWindow : Form
    {
        private readonly GlControl glControl;
        private readonly Stopwatch stopwatch;

        private readonly Game game;

        private float mouseDx, mouseDy;

        public GameWindow()
        {
            SuspendLayout();

            Name = "GameWindow";
            Text = "Mycraft";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;

            glControl = new GlControl
            {
                Name = "GLControl",
                Dock = DockStyle.Fill,

                Animation = true,
                AnimationTimer = false,

                ColorBits = 24u,
                DepthBits = 8u,
                MultisampleBits = 4u,
                StencilBits = 0u
            };

            Resize += OnResized;
            glControl.MouseEnter += (o, e) => Cursor.Hide();
            glControl.MouseMove += OnMouseMove;
            glControl.MouseDown += OnMouseDown;
            glControl.MouseWheel += OnMouseWheel;
            glControl.ContextCreated += OnContextCreated;
            glControl.ContextDestroying += OnContextDestroyed;
            glControl.ContextUpdate += OnContextUpdate;
            glControl.Render += Render;

            Controls.Add(glControl);
            ResumeLayout(false);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            game = new Game();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            game.MoveHotbarSelection(-e.Delta / 120);
        }

        private (float dx, float dy) GrabCursor()
        {
            Point screenLocation = Location;
            Point screenCenter = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
            Point cursorPos = new Point(
                screenLocation.X + screenCenter.X,
                screenLocation.Y + screenCenter.Y
            );

            float dx = Cursor.Position.X - cursorPos.X;
            float dy = Cursor.Position.Y - cursorPos.Y;

            Cursor.Position = cursorPos;
            return (dx, dy);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var (dx, dy) = GrabCursor();
            mouseDx += dx;
            mouseDy += dy;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    game.BreakBlock();
                    break;

                case MouseButtons.Right:
                    game.PlaceBlock();
                    break;
            }
        }
           
        private void OnResized(object sender, EventArgs e)
        {
            if (!Resources.AreLoaded) return;

            Gl.Viewport(0, 0, ClientSize.Width, ClientSize.Height);
            game.Resize(ClientSize.Width, ClientSize.Height);
        }

        private void OnContextCreated(object sender, GlControlEventArgs e)
        {
            Cursor.Hide();
            GrabCursor();

            Resources.LoadAll();
            game.Init();
            game.Resize(ClientSize.Width, ClientSize.Height);
        }

        private const int FPS_UPDATE_RATE = 30;
        private double frameTimeSum;
        private int numFrames;

        private void OnContextUpdate(object sender, GlControlEventArgs e)
        {
            double deltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            frameTimeSum += deltaTime;
            numFrames++;
            if (numFrames >= FPS_UPDATE_RATE)
            {
                Text = $"Mycraft - FPS: {(int)Math.Floor(numFrames / frameTimeSum)}";
                frameTimeSum = 0d;
                numFrames = 0;
            }

            game.MouseInput(
                (float)(mouseDx * deltaTime),
                (float)(mouseDy * deltaTime)
            );
            mouseDx = 0f;
            mouseDy = 0f;

            game.Update(deltaTime);
        }

        private void Render(object sender, GlControlEventArgs e)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            game.Draw();
        }

        private void OnContextDestroyed(object sender, GlControlEventArgs e)
        {
            Resources.DisposeAll();
            game.Dispose();
        }
    }
}
