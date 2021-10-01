using OpenGL;
using System;

namespace Mycraft.Utils
{
    public struct SmoothChangingVertex2f
    {
        public Vertex2f Value { get => currentValue; set => goal = value; }
        public float ChangingSpeed { get; set; }

        private Vertex2f currentValue, goal;

        public SmoothChangingVertex2f(Vertex2f value, float speed)
        {
            currentValue = value;
            goal = value;
            ChangingSpeed = speed;
        }

        public void Update(double deltaTime)
        {
            Vertex2f toGoal = goal - currentValue;
            float delta = (float)(ChangingSpeed * deltaTime);

            if (toGoal.Module() < delta)
                currentValue = goal;
            else
                currentValue += toGoal.Normalized * delta;
        }
    }
}
