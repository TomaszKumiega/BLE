using System;
using System.Timers;

namespace ERGBLE.Services
{
    public class SecondsTimer : ISecondsTimer
    {
        private Timer Timer { get; set; }

        public event EventHandler TimeoutElapsed;
        public event EventHandler IntervalPassed;

        public int TimePassedMs { get; private set; }
        public int IntervalMs { get; set; }
        public int TimeoutMs { get; set; }

        public void Start()
        {
            TimePassedMs = 0;
            Timer = new Timer(IntervalMs);
            Timer.Elapsed += OnIntervalPassed;
            Timer.AutoReset = true;
            Timer.Start();
        }

        private void OnIntervalPassed(object sender, ElapsedEventArgs args)
        {
            TimePassedMs += IntervalMs;

            IntervalPassed?.Invoke(this, EventArgs.Empty);

            if (TimePassedMs >= TimeoutMs)
            {
                Timer.Stop();
                TimeoutElapsed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
