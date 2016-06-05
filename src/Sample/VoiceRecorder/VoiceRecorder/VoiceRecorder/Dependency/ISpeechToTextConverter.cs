using System;
using System.Threading.Tasks;

namespace VoiceRecorder.Dependency
{
    public interface ISpeechToTextConverter : IObservable<SpeechText>, IObserver<StrecmPcm>
    {
        Task Start(string user, string password);
    }

    public class SpeechText
    {
        public bool IsFainel { get; set; }
        public string Text { get; set; }
    }

    public class StrecmPcm
    {
        public bool IsStop { get; set; }
        public byte[] PcmStream { get; set; }
    }
}