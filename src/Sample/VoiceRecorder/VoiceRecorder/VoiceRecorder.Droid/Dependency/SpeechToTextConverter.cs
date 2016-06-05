using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;

namespace VoiceRecorder.Dependency
{
    public class SpeechToTextConverter : ISpeechToTextConverter
    {
        private readonly Subject<SpeechText> _speechTextSubject = new Subject<SpeechText>();

        private WebSocket _client;
        private bool _isConnected;
        private bool _isSending = true;

        public int SampleRate { get; set; } = 16000;
        public int Channel { get; set; } = 1;
        public bool Continuous { get; set; } = true;
        public bool InterimResults { get; set; } = true;


        public IDisposable Subscribe(IObserver<SpeechText> observer)
        {
            return _speechTextSubject.Subscribe(observer);
        }

        public async Task Start(string user, string password)
        {
            var wsUrl =
                $"wss://stream.watsonplatform.net/speech-to-text/api/v1/recognize?model=ja-JP_BroadbandModel";
            _client = new WebSocket(wsUrl);
            _client.SetCredentials(user, password, true);

            _client.OnOpen += (sender, args) => { StartRecognize(); };
            _client.OnError += (sender, args) => { Debug.WriteLine(args.Message); };
            _client.OnClose += (sender, args) => { Debug.WriteLine(args.Reason); };
            _client.OnMessage += (sender, args) =>
            {
                if (args.IsText)
                {
                    var dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(args.Data);

                    if (dic.ContainsKey("state"))
                    {
                        _isConnected = true;
                        Debug.WriteLine(dic["state"]);
                    }
                    else if (dic.ContainsKey("results"))
                    {
                        var result = JsonConvert.DeserializeObject<SpeechResult>(args.Data);
                        Debug.WriteLine(args.Data);
                        result.Results.ForEach(res =>
                        {
                            if (res.Alternatives.Any())
                            {
                                var speechtext = new SpeechText
                                {
                                    IsFainel = res.Final,
                                    Text = res.Alternatives.First().Transcript
                                };
                                _speechTextSubject.OnNext(speechtext);
                            }
                        });
                    }
                    else if (dic.ContainsKey("error"))
                    {
                        Debug.WriteLine(dic["error"]);
                    }
                    else
                    {
                        Debug.WriteLine(args.Data);
                    }
                }
            };
            await Task.Run(() => _client.Connect());
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(StrecmPcm value)
        {
            if (!_isConnected) return;
            if (value.IsStop == false)
            {
                if (!_isSending)
                {
                    StartRecognize();
                }
                _client.Send(value.PcmStream);
            }
            else
            {
                _client.Send("{\"Action\": \"stop\"}");
                _isSending = false;
                Debug.WriteLine("Stop");
            }
        }

        private void StartRecognize()
        {
            var startjson = JsonConvert.SerializeObject(
                new Connection
                {
                    Action = "start",
                    content_Type = $"audio/l16;rate={SampleRate};channels={Channel}",
                    InterimResults = InterimResults,
                    Continuous = Continuous,
                    InactivityTimeout = -1,
                    MaxAlternatives = 1,
                    Timestamps = false,
                    WordConfidence = false
                }, Formatting.Indented);
            Debug.WriteLine(startjson);
            _client.Send(startjson);
            _isSending = true;
        }

        private class Connection
        {
            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("content-type")]
            public string content_Type { get; set; }

            [JsonProperty("continuous")]
            public bool Continuous { get; set; }

            [JsonProperty("interim_results")]
            public bool InterimResults { get; set; }

            [JsonProperty("word_confidence")]
            public bool WordConfidence { get; set; }

            [JsonProperty("timestamps")]
            public bool Timestamps { get; set; }

            [JsonProperty("max_alternatives")]
            public int MaxAlternatives { get; set; }

            [JsonProperty("inactivity_timeout")]
            public int InactivityTimeout { get; set; }
        }

        private class SpeechResult
        {
            public List<Result> Results { get; set; }
        }

        private class Result
        {
            public List<Alternative> Alternatives { get; set; }
            public bool Final { get; set; }
        }

        private class Alternative
        {
            public string Transcript { get; set; }
            public string Confidence { get; set; }
        }
    }
}