using System;

namespace ObservableVoiceCapture.Abstraction
{
    public interface IVoiceSpeaker : IObserver<byte[]>, IDisposable
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
