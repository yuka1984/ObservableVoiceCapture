using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Reactive.Bindings.Notifiers
{
    /// <summary>
    /// Notify of busy.
    /// </summary>
    public class BusyNotifier : INotifyPropertyChanged, IObservable<bool>
    {
        private static readonly PropertyChangedEventArgs IsBusyPropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(IsBusy));

        /// <summary>
        /// property changed event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private int ProcessCounter
        {
            get { return _processCounter; }
            set
            {
                bool busychange = (value > 0 && _processCounter <= 0) || (value <= 0 && _processCounter > 0);
                _processCounter = value;
                if (busychange)
                {
                    PropertyChanged?.Invoke(this, IsBusyPropertyChangedEventArgs);
                    IsBusySubject.OnNext(_processCounter > 0);
                }
            }
        }

        private int _processCounter;

        private Subject<bool> IsBusySubject { get; } = new Subject<bool>();

        private object LockObject { get; } = new object();

        /// <summary>
        /// Is process running.
        /// </summary>
        public bool IsBusy
        {
            get { return this.ProcessCounter > 0; }
            set
            {
                if (value)
                {
                    if (this.ProcessCounter > 0) return;
                    this.ProcessCounter++;
                }
                else
                {
                    if (this.ProcessCounter <= 0) return;
                    this.ProcessCounter--;
                }
            }
        }

        /// <summary>
        /// Process start.
        /// </summary>
        /// <returns>Call dispose method when process end.</returns>
        public IDisposable ProcessStart()
        {
            lock (this.LockObject)
            {
                this.ProcessCounter++;
                return Disposable.Create(() =>
                {
                    lock (this.LockObject)
                    {
                        this.ProcessCounter--;
                    }
                });
            }
        }

        /// <summary>
        /// Subscribe busy.
        /// </summary>
        /// <param name="observer">observer</param>
        /// <returns>disposable</returns>
        public IDisposable Subscribe(IObserver<bool> observer)
        {
            observer.OnNext(this.IsBusy);
            return this.IsBusySubject.Subscribe(observer);
        }
    }
}