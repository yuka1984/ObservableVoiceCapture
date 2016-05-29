using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ObservableVoiceCapture.Abstraction;

namespace ObservableVoiceCapture.Test
{
    [TestFixture]
    public class VoiceCaptureTest
    {
        private IVoiceCapture _capture;

        [SetUp]
        public void Setup()
        {
            _capture = new VoiceCapture(8000, 250);
        }

        [TearDown]
        public void Tear()
        {
            _capture.Dispose();
        }

        [Test]
        public void CaptureTest()
        {
            var _stopwatch = Stopwatch.StartNew();
            _capture.Subscribe(b =>
            {
                var time = _stopwatch.ElapsedMilliseconds;
                var bs = string.Join("-", b.Select(x => x.ToString("x2")));
                Console.WriteLine(bs);
                Debug.WriteLine($"{time} {b.Length} {bs}");
            });
            _capture.Start();
            Thread.Sleep(5000);
        }
    }
}
