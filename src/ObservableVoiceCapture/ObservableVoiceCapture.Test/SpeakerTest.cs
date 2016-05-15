using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AudioToolbox;
using NUnit.Framework;

namespace ObservableVoiceCapture.Test
{
    [TestFixture]
    public class SpeakerTest
    {
        private IVoiceCapture _capture;
        private IVoiceSpeaker _speaker;

        [SetUp]
        public void Setup()
        {
            AudioSession.Initialize();
            AudioSession.SetActive(true);
            AudioSession.Category = AudioSessionCategory.PlayAndRecord;

            _capture = new VoiceCapture(8000, 250);
            _speaker = new VoiceSpeaker(8000);
        }

        [TearDown]
        public void Tear()
        {
            _capture.Dispose();
            AudioSession.SetActive(false);
        }

        [Test]
        public void CaptureSpeakerTest()
        {
            var _stopwatch = Stopwatch.StartNew();
            _capture.Subscribe(b =>
            {
                var time = _stopwatch.ElapsedMilliseconds;
                var bs = string.Join("-", b.Select(x => x.ToString("x2")));
                Console.WriteLine(bs);
                Debug.WriteLine($"{time} {b.Length} {bs}");
            });
            _capture.Delay(TimeSpan.FromMilliseconds(1000)).Subscribe(_speaker);
            _capture.Start();
            Thread.Sleep(5000);
        }
    }
}
