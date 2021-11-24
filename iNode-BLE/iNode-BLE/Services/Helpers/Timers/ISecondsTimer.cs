using System;

namespace ERGBLE.Services
{
    public interface ISecondsTimer
    {
        int IntervalMs { get; set; }
        int TimeoutMs { get; set; }
        int TimePassedMs { get; }

        event EventHandler IntervalPassed;
        event EventHandler TimeoutElapsed;

        void Start();
    }
}