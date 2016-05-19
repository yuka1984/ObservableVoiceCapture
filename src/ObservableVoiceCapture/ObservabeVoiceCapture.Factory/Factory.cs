using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObservableVoiceCapture;

namespace ObservableVoiceCapture
{
    public class CaptureFactory
    {
        public static IVoiceCapture Create(int sampleSize, int buffermilliseconds)
        {
#if PORTABLE
            throw new NotImplementedException();
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
