using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mycraft.Utils
{
    public class Profiler
    {
        public long FrameTime => frameStopwatch.ElapsedMilliseconds;

        private readonly Stopwatch frameStopwatch;
        private readonly Stopwatch fragmentStopwatch;
        private readonly Dictionary<string, long> fragments;

        public Profiler()
        {
            frameStopwatch = new Stopwatch();
            fragmentStopwatch = new Stopwatch();
            fragments = new Dictionary<string, long>();
        }

        public void NewFrame()
        {
            fragments.Clear();
            frameStopwatch.Restart();
            fragmentStopwatch.Restart();
        }

        public void EndFragment(string name)
        {
            fragments.Add(name, fragmentStopwatch.ElapsedMilliseconds);
            fragmentStopwatch.Restart();
        }

        public void EndFrame()
        {
            frameStopwatch.Stop();
            fragmentStopwatch.Stop();
        }

        public void PrintInfo()
        {
            if (frameStopwatch.IsRunning)
            {
                EndFrame();
            }

            Console.WriteLine("Total frame time: " + frameStopwatch.ElapsedMilliseconds);
            foreach (var pair in fragments)
            {
                Console.WriteLine($"{pair.Key} time: {pair.Value}");
            }
        }
    }
}
