using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FormsToolkit;
using Microsoft.Practices.ObjectBuilder2;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using VoiceRecorder.Dependency;
using Xamarin.Forms;

namespace VoiceRecorder.ViewModels
{
    public class ListPageViewModel : IDisposable
    {
        private readonly IFile _file;

        public ListPageViewModel(IFile file, BusyNotifier busi)
        {
            _file = file;
            Busy = busi;

            AddCommand = Busy.Inverse().ToReactiveCommand();
            AddCommand.Subscribe(z => { MessagingService.Current.SendMessage("RecordPage"); });
            DeleteCommand = Busy.Inverse().ToReactiveCommand<string>();
            DeleteCommand.Subscribe(x =>
            {
                _file.Delete(x, StorageType.Shared);
                if (BackgrouundLoadListCommand.CanExecute())
                {
                    BackgrouundLoadListCommand.Execute();
                }
            });
            OpenFileCommand = Busy.Inverse().ToReactiveCommand<string>();
            OpenFileCommand.Subscribe(x => { _file.OpenWavFile(x); });

            LoadListCommand = new ReactiveCommand();
            LoadListCommand.Subscribe(x =>
            {
                LoadList();
                Busy.IsBusy = false;
            });

            BackgrouundLoadListCommand = Busy.Inverse().ToReactiveCommand();
            BackgrouundLoadListCommand.Subscribe(x =>
            {
                using (Busy.ProcessStart())
                {
                    LoadList();
                }
            });

            SelectCommand = Busy.Inverse().ToReactiveCommand<ListItem>();
            SelectCommand.Subscribe(x =>
            {
                using (Busy.ProcessStart())
                {
                    _file.OpenWavFile(x.FullFilePath);
                }
            });

            MessagingCenter.Subscribe<RecordPageViewModel>(this, "LoadList",
                s => { BackgrouundLoadListCommand.Execute(); });

            LoadList();
        }

        public ObservableCollection<ListItem> List { get; } = new ObservableCollection<ListItem>();
        public ReactiveCommand AddCommand { get; }
        public ReactiveCommand<string> DeleteCommand { get; }
        public ReactiveCommand<string> OpenFileCommand { get; }
        public ReactiveCommand LoadListCommand { get; }
        public ReactiveCommand BackgrouundLoadListCommand { get; }
        public ReactiveCommand<ListItem> SelectCommand { get; }
        public BusyNotifier Busy { get; }

        public void Dispose()
        {
            MessagingCenter.Unsubscribe<RecordPageViewModel>(this, "LoadList");
        }

        private void LoadList()
        {
            List.Clear();
            _file.GetFiles(StorageType.Shared)
                .Where(x => x.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase))
                .Select(x => new ListItem
                {
                    FileName = x.Name,
                    Detail = $"{x.CreationTimeUtc.ToLocalTime():yyyy/MMMM/dd hh:mm:ss} {x.Length}bytes",
                    FullFilePath = x.FullName,
                    DeleteCommand = DeleteCommand,
                    SheredCommand = OpenFileCommand
                }).ToList().ForEach(x => List.Add(x));
        }
    }

    public class ListItem
    {
        public string FileName { get; set; }
        public string Detail { get; set; }
        public string FullFilePath { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SheredCommand { get; set; }
    }
}