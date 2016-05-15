using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableVoiceCapture
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
