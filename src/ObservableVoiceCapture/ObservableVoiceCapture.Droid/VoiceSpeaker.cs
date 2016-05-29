using System;
using Android.Media;
using ObservableVoiceCapture.Abstraction;

namespace ObservableVoiceCapture
{
    public class VoiceSpeaker : IVoiceSpeaker
    {
        private readonly AudioTrack _track;
        private bool _isplay;

        public VoiceSpeaker(int sampleSize, Bits bits = Bits.Pcm16, Channel channel = Channel.Mono)
        {
            _track = new AudioTrack(
                Stream.Music
                , sampleSize
                , channel == Channel.Mono ? ChannelOut.Mono : ChannelOut.Stereo
                , bits == Bits.Pcm16 ? Encoding.Pcm16bit : Encoding.Pcm8bit
                , AudioTrack.GetMinBufferSize(
                    sampleSize
                    , channel == Channel.Mono ? ChannelOut.Mono : ChannelOut.Stereo
                    , bits == Bits.Pcm16 ? Encoding.Pcm16bit : Encoding.Pcm8bit)
                , AudioTrackMode.Stream);
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
            _track.Write(value, 0, value.Length);
        }

        public void Dispose()
        {
            Stop();
            _track.Dispose();
        }

        public void Start()
        {
            if (_isplay) return;
            _track.Play();
            _isplay = true;
        }

        public void Stop()
        {
            if (!_isplay) return;
            _track.Stop();
            _track.Flush();
            _isplay = false;
        }
    }
}