using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using AudioToolbox;
using ObservableVoiceCapture.Abstraction;

namespace ObservableVoiceCapture
{
    public class VoiceCapture : IVoiceCapture, IDisposable
    {
        private readonly Subject<byte[]> _subject = new Subject<byte[]>();
        private readonly InputAudioQueue _audioQueue;
        private readonly List<IntPtr> _bufferPtrs = new List<IntPtr>();
        private readonly int _pushsize;
        private bool _isrecording;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sampleSize">SampleSize</param>
        /// <param name="buffermilliseconds">Milliseconds for the buffer</param>
        public VoiceCapture(int sampleSize, int buffermilliseconds)
        {
            if (buffermilliseconds > 1000) throw new ArgumentOutOfRangeException(nameof(buffermilliseconds));
            _pushsize = sampleSize/(1000/buffermilliseconds);

            var description = new AudioStreamBasicDescription
            {
                SampleRate = sampleSize,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsPacked | AudioFormatFlags.IsSignedInteger,
                BitsPerChannel = 16,
                ChannelsPerFrame = 1,
                BytesPerFrame = 2,
                FramesPerPacket = 1,
                BytesPerPacket = 2,
                Reserved = 0
            };

            _audioQueue = new InputAudioQueue(description);
            for (var i = 0; i < 3; i++)
            {
                IntPtr ptr;
                _audioQueue.AllocateBufferWithPacketDescriptors(_pushsize*description.BytesPerPacket, _pushsize, out ptr);
                _audioQueue.EnqueueBuffer(ptr, _pushsize, null);
                _bufferPtrs.Add(ptr);
            }
            _audioQueue.InputCompleted += AudioQueueOnInputCompleted;
        }

        public void Dispose()
        {
            Stop();
            _subject.OnCompleted();
            foreach (var ptr in _bufferPtrs)
            {
                _audioQueue.FreeBuffer(ptr);
            }
            _audioQueue.QueueDispose();
            _audioQueue.Dispose();
            _subject.Dispose();
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _subject.Subscribe(observer);

        public void Start()
        {
            if (_isrecording) return;
            var status = _audioQueue.Start();
            if (status != AudioQueueStatus.Ok)
            {
                throw new Exception(status.ToString());
            }
            _isrecording = true;
        }

        public void Stop()
        {
            if (!_isrecording) return;
            _audioQueue.Stop(true);
            _isrecording = false;
        }

        private void AudioQueueOnInputCompleted(object sender, InputCompletedEventArgs args)
        {
            var buffer = (AudioQueueBuffer) Marshal.PtrToStructure(args.IntPtrBuffer, typeof(AudioQueueBuffer));
            var send = new byte[buffer.AudioDataByteSize];
            Marshal.Copy(buffer.AudioData, send, 0, (int) buffer.AudioDataByteSize);
            _subject.OnNext(send);

            _audioQueue.EnqueueBuffer(args.IntPtrBuffer, _pushsize, args.PacketDescriptions);
        }
    }
}