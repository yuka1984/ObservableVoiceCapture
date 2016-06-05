using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BitConverterExtention;
using ObservableVoiceCapture;
using ObservableVoiceCapture.Abstraction;
using Prism.Mvvm;
using VoiceRecorder.Dependency;

namespace VoiceRecorder.Core
{
    public class Recorder : BindableBase, IDisposable
    {
        private const int SampleSize = 16000;
        private const int BufferTime = 100;
        private const int TimeSize = SampleSize*2/1000; // 1msecあたりのPCMサイズ
        private const string tempFileName = "tempsound";
        private readonly IVoiceCapture _capture;
        private readonly IFile _file;
        private readonly object lockobj = new object();

        private bool _isrecording;

        private bool _issilencCut = true;

        private bool _isstarted;
        private long _positiontime;

        private readonly ISpeechToTextConverter _speechToTextConverter;

        private int _stopframecount;
        private Stream _tempStream;

        private long _totaltime;

        public Recorder(IFile file, ISpeechToTextConverter speechToTextConverter)
        {
            _capture = CaptureFactory.Get(SampleSize, BufferTime);
            _speechToTextConverter = speechToTextConverter;
            RecognizedVoices = new ReadOnlyObservableCollection<string>(_recognizedvoices);
            var noiseCut = _capture
                .Select(x =>
                {
                    var capcturing = IsCapturing;
                    for (var i = 0; i < x.Length; i += 2)
                    {
                        var test = BitConverter.ToInt16(x, i);
                        if ((test >= 0 && 100 > test) || (test <= 0 && -100 < test))
                        {
                            x[i] = 0;
                            x[i + 1] = 0;
                        }
                    }
                    if (x.Count(y => y != 0) > x.Length*0.3)
                    {
                        StopFrameCount = 0;
                    }
                    else
                    {
                        if (StopFrameCount < int.MaxValue)
                        {
                            StopFrameCount++;
                        }
                    }
                    if (capcturing && !IsCapturing)
                    {
                        speechToTextConverter.OnNext(new StrecmPcm {IsStop = true});
                    }

                    return x;
                });
            _speechToTextConverter.Subscribe(x =>
            {
                if (x.IsFainel)
                {
                    _recognizedvoices.Add(x.Text);
                }
                CurrentVoice = x.Text;
            });


            noiseCut.Subscribe(x =>
            {
                if (_tempStream != null && _tempStream.CanWrite)
                {
                    if (IsCapturing)
                    {
                        _tempStream.Write(x, 0, x.Length);
                        speechToTextConverter.OnNext(new StrecmPcm { IsStop = false, PcmStream = x});
                        UpdateTime();
                    }
                }
            });
            _file = file;
        }

        public bool IsSilenceCut
        {
            get { return _issilencCut; }
            set
            {
                if (SetProperty(ref _issilencCut, value))
                {
                    OnPropertyChanged(nameof(IsCapturing));
                }
            }
        }

        public bool IsStarted
        {
            get { return _isstarted; }
            private set { SetProperty(ref _isstarted, value); }
        }

        public bool IsRecording
        {
            get { return _isrecording; }
            private set { SetProperty(ref _isrecording, value); }
        }

        public int StopFrameCount
        {
            get { return _stopframecount; }
            private set
            {
                if (SetProperty(ref _stopframecount, value))
                {
                    OnPropertyChanged(nameof(IsCapturing));
                }
            }
        }

        public bool IsCapturing => IsRecording && (!IsSilenceCut || StopFrameCount < 10);


        public long PositionTime
        {
            get { return _positiontime; }
            private set { SetProperty(ref _positiontime, value); }
        }

        public long TotalTime
        {
            get { return _totaltime; }
            private set { SetProperty(ref _totaltime, value); }
        }

        public void Dispose()
        {
            _capture.Dispose();
            _tempStream?.Dispose();
        }

        public async Task Start()
        {
            if (IsRecording) return;
            if (!IsStarted)
            {
                _tempStream?.Dispose();
                _tempStream = _file.GetStream(tempFileName, StorageType.Local, FileMode.Create, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
            }
            await _speechToTextConverter.Start("*", "*");
            _capture.Start();
            IsRecording = true;
            IsStarted = true;
            StopFrameCount = int.MaxValue;
        }

        public void Stop()
        {
            lock (lockobj)
            {
                if (!IsRecording) return;
                _capture.Stop();
                IsRecording = false;
            }
        }

        public void Save(string name)
        {
            lock (lockobj)
            {
                var filename = $"{name}.wav";
                if (!_file.Exsists(filename, StorageType.Shared))
                {
                    using (
                        var savestream = _file.GetStream(filename, StorageType.Shared, FileMode.Create, FileAccess.Write,
                            FileShare.Write))
                    {
                        var header = WavHeader.Get((uint) _tempStream.Length, 1, SampleSize, 16);
                        savestream.Write(header.ToByteArray(), 0, Marshal.SizeOf(header));
                        _tempStream.Position = 0;
                        var buffer = new byte[_tempStream.Length];
                        _tempStream.Read(buffer, 0, buffer.Length);
                        savestream.Write(buffer, 0, buffer.Length);
                        savestream.Flush();
                    }
                    _tempStream.Dispose();
                    _file.Delete(tempFileName, StorageType.Local);
                }
            }
        }

        private string _currentvoice;
        public string CurrentVoice
        {
            get { return _currentvoice; }
            set { SetProperty(ref _currentvoice, value); }
        }

        private readonly ObservableCollection<string> _recognizedvoices = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> RecognizedVoices { get; private set; }


        private void UpdateTime()
        {
            TotalTime = _tempStream?.Length/TimeSize ?? 0;
            PositionTime = _tempStream?.Position/TimeSize ?? 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WavHeader
        {
            public static WavHeader Get(uint payloadSize, ushort channels, uint sampleSize, ushort bits)
            {
                var result = new WavHeader
                {
                    riffID = "RIFF".ToByteArray(),
                    size = Convert.ToUInt32(payloadSize + Marshal.SizeOf(typeof(WavHeader)) - 8),
                    wavID = "WAVE".ToByteArray(),
                    fmtID = "fmt ".ToByteArray(),
                    fmtSize = 16,
                    format = 1,
                    channels = channels,
                    sampleRate = sampleSize,
                    bytePerSec = Convert.ToUInt32(sampleSize*(bits/8)*channels),
                    blockSize = (ushort) (bits/8),
                    bit = bits,
                    dataID = "data".ToByteArray(),
                    dataSize = payloadSize
                };

                return result;
            }

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] riffID; // "riff"
            public uint size; // ファイルサイズ-8
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] wavID; // "WAVE"
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] fmtID; // "fmt "
            public uint fmtSize; // fmtチャンクのバイト数
            public ushort format; // フォーマット
            public ushort channels; // チャンネル数
            public uint sampleRate; // サンプリングレート
            public uint bytePerSec; // データ速度
            public ushort blockSize; // ブロックサイズ
            public ushort bit; // 量子化ビット数
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] dataID; // "data"
            public uint dataSize; // 波形データのバイト数
        }
    }
}