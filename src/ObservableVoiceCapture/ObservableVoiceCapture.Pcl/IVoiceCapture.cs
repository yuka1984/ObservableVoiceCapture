using System;

namespace ObservableVoiceCapture.Abstraction
{
    public interface IVoiceCapture : IObservable<byte[]>, IDisposable
    {
        /// <summary>
        /// CaptureStart
        /// </summary>
        void Start();

        /// <summary>
        /// CaptureStop
        /// </summary>
        void Stop();
    }
}
