using System;
using System.Timers;
using Eto.Forms;

namespace PathFinder.Gui
{
    public class UiEventDebouncer<T> : IDisposable where T : class
    {
        public event EventHandler<T> Fired;
        private System.Threading.Timer _timer;
        private T _lastArgs;
        private object _lastSender;
        private int _timeout;
        
        public UiEventDebouncer(int timeMs)
        {
            _timeout = timeMs;
        }

        public void Handle(object sender, T args)
        {
            _lastSender = sender;
            _lastArgs = args;
            _timer?.Dispose();
            _timer = new System.Threading.Timer(TimerFired, null, _timeout, int.MaxValue);
        }

        private void TimerFired(object sender) => Application.Instance.InvokeAsync(InvokeFire);

        private void InvokeFire()
        {
            _timer.Dispose();
            _timer = null;
            Fired?.Invoke(_lastSender, _lastArgs);
            _lastSender = null;
            _lastArgs = null;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}