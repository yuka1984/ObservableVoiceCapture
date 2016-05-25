using System;
using System.Reactive.Subjects;
using System.Threading;
using Android;
using Android.App;
using Android.Media;
using ObservableVoiceCapture.Abstraction;

[assembly: UsesPermission(Manifest.Permission.RecordAudio)]
namespace ObservableVoiceCapture
{
    public class VoiceCapture : IVoiceCapture, IDisposable
    {
        private readonly short[] _buffer;
        private readonly Subject<byte[]> _readSubject = new Subject<byte[]>();
        private readonly AudioRecord _record;
        private bool _isrecording;
        private Thread _thread;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sampleSize">SampleSize</param>
        /// <param name="buffermilliseconds">Milliseconds for the buffer</param>
        public VoiceCapture(int sampleSize, int buffermilliseconds)
        {
            if (buffermilliseconds > 1000) throw new ArgumentOutOfRangeException(nameof(buffermilliseconds));
            var pushsize = sampleSize/(1000/buffermilliseconds);
            var minbuffersize = AudioRecord.GetMinBufferSize(sampleSize, ChannelIn.Mono, Encoding.Pcm16bit);
            if (pushsize < minbuffersize)
                throw new ArgumentException($"MinBufferSize is {minbuffersize}byte");
            _record = new AudioRecord(AudioSource.Default, sampleSize, ChannelIn.Mono, Encoding.Pcm16bit, pushsize);
            _buffer = new short[pushsize/2];
        }

        public void Dispose()
        {
            Stop();
            _record.Release();
            _readSubject.OnCompleted();
            _readSubject.Dispose();
            _record.Dispose();
        }

        /// <summary>
        ///     CaptureStart
        /// </summary>
        public void Start()
        {
            if (_isrecording) return;
            _isrecording = true;
            _thread = new Thread(ReadThread);
            _thread.Start();
        }

        /// <summary>
        ///     CaptureStop
        /// </summary>
        public void Stop()
        {
            if (!_isrecording) return;
            _isrecording = false;
            Thread.Sleep(1000);
            _thread.Abort();
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _readSubject.Subscribe(observer);

        private void ReadThread()
        {
            _record.StartRecording();
            while (_isrecording)
            {
                var size = _record.Read(_buffer, 0, _buffer.Length);
                var result = new byte[size*2];
                Buffer.BlockCopy(_buffer, 0, result, 0, result.Length);
                _readSubject.OnNext(result);
            }
            _record.Stop();
        }
    }
}