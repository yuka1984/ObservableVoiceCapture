using System;
using System.Reactive.Linq;
using FormsToolkit;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using VoiceRecorder.Core;

namespace VoiceRecorder.ViewModels
{
    public class RecordPageViewModel
    {
        private readonly Recorder _recorder;

        public RecordPageViewModel(BusyNotifier busy, Recorder recorder)
        {
            _recorder = recorder;

            PositionTime = _recorder
                .ToReactivePropertyAsOneWaySync(x => x.PositionTime,
                    x => TimeSpan.FromMilliseconds(x).ToString(@"hh\:mm\:ss"));

            TotalTime = _recorder
                .ToReactivePropertyAsOneWaySync(x => x.TotalTime,
                    x => TimeSpan.FromMilliseconds(x).ToString(@"hh\:mm\:ss"));

            IsStarted = _recorder.ToReactivePropertyAsOneWaySync(x => x.IsStarted);
            IsRecording = _recorder.ToReactivePropertyAsOneWaySync(x => x.IsRecording);
            IsCapturing = _recorder.ToReactivePropertyAsOneWaySync(x => x.IsCapturing);
            IsSilenceCut = _recorder.ToReactivePropertyAsSynchronized(x => x.IsSilenceCut);

            StatusMessage = IsStarted.CombineLatest(IsRecording, IsCapturing, (start, recording, capturing) =>
            {
                if (!start) return "STOP";
                if (recording)
                {
                    if (capturing)
                    {
                        return "Recording...";
                    }
                    return "Waiting...";
                }
                return "PAUSE";
            }).ToReadOnlyReactiveProperty();

            StartCommand = IsRecording.Inverse().ToReactiveCommand();
            StartCommand.Subscribe(x => recorder.Start());

            StopCommand = IsRecording.ToReactiveCommand();
            StopCommand.Subscribe(x => recorder.Stop());

            ResumeCommand = IsStarted
                .CombineLatest(IsRecording.Inverse(), (x, y) => x && y)
                .ToReactiveCommand();
            ResumeCommand.Subscribe(x => recorder.Start());

            SaveCommand = IsStarted
                .CombineLatest(IsRecording.Inverse(), busy.Inverse(), (x, y, z) => x && y && z)
                .ToReactiveCommand();
            SaveCommand.Subscribe(x =>
            {
                using (busy.ProcessStart())
                {
                    recorder.Save(DateTime.Now.ToString("yyyyMMddhhmmss"));
                    MessagingService.Current.SendMessage(MessageKey.RecordPageClose);
                    MessagingService.Current.Unsubscribe(MessageKey.GoBack);
                }
            });
            CloseCommand = IsRecording.Inverse().CombineLatest(busy.Inverse(), (x, y) => x && y)
                .ToReactiveCommand();
            CloseCommand.Subscribe(x =>
            {
                using (busy.ProcessStart())
                {
                    if (IsStarted.Value)
                    {
                        MessagingService.Current.SendMessage(MessageKey.Question,
                            new MessagingServiceQuestion
                            {
                                Title = "Recording"
                                ,
                                Question = "Already to start recording . Stop recording without saving ?"
                                ,
                                Positive = "Yes"
                                ,
                                Negative = "No"
                                ,
                                OnCompleted = b =>
                                {
                                    if (b)
                                    {
                                        MessagingService.Current.SendMessage(MessageKey.RecordPageClose);
                                        MessagingService.Current.Unsubscribe(MessageKey.GoBack);
                                    }
                                }
                            });
                    }
                    else
                    {
                        MessagingService.Current.SendMessage(MessageKey.RecordPageClose);
                        MessagingService.Current.Unsubscribe(MessageKey.GoBack);
                    }
                }
            });
            MessagingService.Current.Subscribe(MessageKey.GoBack, service =>
            {
                if (CloseCommand.CanExecute())
                {
                    CloseCommand.Execute();
                }
            });
        }

        public ReadOnlyReactiveProperty<string> PositionTime { get; private set; }
        public ReadOnlyReactiveProperty<string> TotalTime { get; private set; }
        public ReadOnlyReactiveProperty<bool> IsStarted { get; }
        public ReadOnlyReactiveProperty<bool> IsRecording { get; }
        public ReadOnlyReactiveProperty<bool> IsCapturing { get; }
        public ReactiveProperty<bool> IsSilenceCut { get; private set; }
        public ReadOnlyReactiveProperty<string> StatusMessage { get; private set; }

        public ReactiveCommand StartCommand { get; }
        public ReactiveCommand StopCommand { get; }
        public ReactiveCommand ResumeCommand { get; }
        public ReactiveCommand SaveCommand { get; }
        public ReactiveCommand CloseCommand { get; }
    }
}