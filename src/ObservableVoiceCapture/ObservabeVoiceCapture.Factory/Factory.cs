using System;
using ObservableVoiceCapture.Abstraction;

namespace ObservableVoiceCapture
{
    public static class CaptureFactory
    {
        public static IVoiceCapture Get(int sampleSize, int buffermilliseconds)
        {
            var lazy = new Lazy<IVoiceCapture>(()=> Create(sampleSize, buffermilliseconds));
            if (lazy.Value == null)
            {
                throw new NotImplementedException();
            }
            return lazy.Value;
        }        

        private static IVoiceCapture Create(int sampleSize, int buffermilliseconds)
        {
#if PORTABLE
            return null;
#else
            return new VoiceCapture(sampleSize, buffermilliseconds);
#endif
        }
    }

    public class SpeakerFactory
    {
        public static IVoiceSpeaker Create(int sampleSize, Bits bits = Bits.Pcm16, Channel channel = Channel.Mono)
        {
#if PORTABLE
            throw new NotImplementedException();
#else
            return new VoiceSpeaker(sampleSize, bits, channel);
#endif
        }
    }
}
