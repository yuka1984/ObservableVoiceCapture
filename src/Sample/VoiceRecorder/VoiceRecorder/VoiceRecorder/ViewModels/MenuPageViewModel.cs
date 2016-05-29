using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FormsToolkit;
using Reactive.Bindings;

namespace VoiceRecorder.ViewModels
{
    public class MenuPageViewModel
    {
        public MenuPageViewModel()
        {
            Items = new List<MenuItem>
            {
                new MenuItem
                {
                    Text = "List",
                    Detail = "保存されている録音データ一覧を表示します",
                    Navigation = "ListPage"
                },
                new MenuItem
                {
                    Text = "License",
                    Detail = "ライセンスを表示します",
                    Navigation = "License"
                }
            };
            SelectedItem.Value = Items.First();
            SelectedItem.Where(x => x != null).Subscribe(x => { MessagingService.Current.SendMessage(x.Navigation); });
        }

        public List<MenuItem> Items { get; set; }
        public ReactiveProperty<MenuItem> SelectedItem { get; } = new ReactiveProperty<MenuItem>();
    }

    public class MenuItem
    {
        public string Text { get; set; }
        public string Detail { get; set; }
        public string Navigation { get; set; }
    }
}