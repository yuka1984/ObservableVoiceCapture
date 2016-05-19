using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using ObservableVoiceCapture;

namespace ObservableVoiceCapture
{
    public class VoiceSpeaker : IVoiceSpeaker, IDisposable
    {
        private readonly IWavePlayer _player;
        private readonly BufferedWaveProvider _provider;
        private bool _isplay;

        public VoiceSpeaker(int sampleSize, Bits bits = Bits.Pcm16, Channel channels = Channel.Mono)
            : this(sampleSize, (int) bits, (int) channels)
        {
            
        }

        private VoiceSpeaker(int sampleSize, int bits = 16, int channels = 1)
        {
            _provider = new BufferedWaveProvider(new WaveFormat(sampleSize, bits, channels));
            _player = new WaveOutEvent();
            _player.Init(_provider);
            _isplay = false;
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
            _provider.AddSamples(value, 0, value.Length);
        }

        public void Start()
        {
            if(_isplay) return;
            _player.Play();
            _isplay = true;
        }

        public void Stop()
        {
            if (!_isplay) return;
            _player.Stop();
            _isplay = false;
        }

        public void Dispose()
        {
            Stop();
            _player.Dispose();
        }
    }
}
