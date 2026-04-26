using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EventTimerOverlay
{
    public class TimerViewModel : INotifyPropertyChanged
    {
        private TimeSpan _remaining;
        private TimeSpan _total;

        public TimeSpan Remaining
        {
            get => _remaining;
            set { _remaining = value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); }
        }

        public TimeSpan Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); }
        }

        public double Progress =>
            Total.TotalSeconds == 0 ? 0 :
            Remaining.TotalSeconds / Total.TotalSeconds;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}