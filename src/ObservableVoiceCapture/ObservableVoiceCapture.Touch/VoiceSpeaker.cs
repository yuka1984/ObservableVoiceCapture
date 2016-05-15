using System;
using System.Collections.Generic;
using System.Linq;
using AudioToolbox;

namespace ObservableVoiceCapture
{
    public class VoiceSpeaker : IVoiceSpeaker
    {
        private const int MaxBufferSize = 32768;
        private readonly OutputAudioQueue _audioQueue;
        private readonly List<IntPtr> _bufferPtrs = new List<IntPtr>();
        private readonly Queue<byte[]> _streamQueue = new Queue<byte[]>();
        private readonly byte[] EmptyBuffer = new byte[20];
        private readonly object queurLock = new object();

        public VoiceSpeaker(int sampleSize, Bits bits = Bits.Pcm16, Channel channel = Channel.Mono)
        {
            var bitsPerChangel = (int) bits;
            var channels = (int) channel;

            var description = new AudioStreamBasicDescription
            {
                SampleRate = sampleSize,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsPacked | AudioFormatFlags.IsSignedInteger,
                BitsPerChannel = bitsPerChangel,
                ChannelsPerFrame = channels,
                BytesPerFrame = bitsPerChangel/8*channels,
                FramesPerPacket = 1,
                Reserved = 0
            };
            description.BytesPerPacket = description.BytesPerFrame*description.FramesPerPacket;

            _audioQueue = new OutputAudioQueue(description);
            _audioQueue.BufferCompleted += AudioQueueOnBufferCompleted;


            for (var i = 0; i < 3; i++)
            {
                IntPtr ptr;
                _audioQueue.AllocateBuffer(MaxBufferSize, out ptr);
                _bufferPtrs.Add(ptr);
                OutputCallback(ptr);
            }
        }

        public void OnCompleted()
        {
            Stop();
        }

        public void OnError(Exception error)
        {
            Stop();
        }

        public void OnNext(byte[] value)
        {
            Start();
            var overCount = value.Length/MaxBufferSize;
            for (var i = 0; i < overCount; i++)
            {
                var bytes = new byte[MaxBufferSize];
                Buffer.BlockCopy(value, MaxBufferSize*i, bytes, 0, MaxBufferSize);
                lock (queurLock)
                {
                    _streamQueue.Enqueue(bytes);
                }
            }
            var remainsize = value.Length%MaxBufferSize;
            if (remainsize > 0)
            {
                var bytes = new byte[remainsize];
                Buffer.BlockCopy(value, MaxBufferSize*overCount, bytes, 0, remainsize);
                lock (queurLock)
                {
                    _streamQueue.Enqueue(bytes);
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _audioQueue.BufferCompleted -= AudioQueueOnBufferCompleted;
            _bufferPtrs.ForEach(x => _audioQueue.FreeBuffer(x));
            _audioQueue.Dispose();
        }

        public void Start()
        {
            if (_audioQueue.IsRunning) return;
            var status = _audioQueue.Start();
            if (status != AudioQueueStatus.Ok)
            {
                throw new Exception(status.ToString());
            }
        }

        public void Stop()
        {
            if (!_audioQueue.IsRunning) return;
            _audioQueue.Stop(false);
        }

        private void AudioQueueOnBufferCompleted(object sender, BufferCompletedEventArgs args)
        {
            OutputCallback(args.IntPtrBuffer);
        }

        private unsafe void OutputCallback(IntPtr ptr)
        {
            byte[] sd;
            lock (queurLock)
            {
                sd = _streamQueue.Any() ? _streamQueue.Dequeue() : EmptyBuffer;
            }
            fixed (byte* bf = sd)
            {
                AudioQueue.FillAudioData(ptr, 0, new IntPtr(bf), 0, sd.Length);
            }
            _audioQueue.EnqueueBuffer(ptr, sd.Length, null);
        }
    }
}